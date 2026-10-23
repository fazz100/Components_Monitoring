# APPLICATION_GROUP_MEMBERSHIP_INSTRUCTIONS.md — Application Group Membership Table

> **Target Execution Environment:** Claude / AI Coding Assistant  
> **Context:** This document records the change that turned `application_group` into a pure lookup table and moved application-to-group membership into a junction table, `application_group_applications`. All rules, conventions, and constraints in `CLAUDE.md` apply (.NET Framework 4.7.2, C# 7.3, Dapper, classic `.csproj`, synchronous methods, `Pascal_Snake_Case` property mapping).  
> **Status:** Implemented on branch `components-monitoring-sp-01-components-detection`. Run after `APPLICATION_GROUP_FEATURES_INSTRUCTIONS.md` (migration `001_…`).

---

## 1. Overview & Objectives

1. **`application_group` is a lookup (reference) table** of systems. Only the Application Groups CRUD page (`src/views/application-group.jsx`) writes to it.
2. **Membership lives in `application_group_applications`.**
   - When an application on the Applications page (`src/views/application-list.jsx`) is assigned a system, the pairing is saved as a row in this table.
   - When the application is set to no system ("None (Standalone)"), the row is removed.
   - An application is allowed to have no system.

---

## 2. Original Request

> `application_group` is supposed to be a lookup or reference table, to be written by the CRUD `application-group.jsx` page for the group. Change the implementation to match this:
>
> ```sql
> create table [application_group]
> (
> id nvarchar(50) not null PRIMARY KEY
> ,[application_group_name] nvarchar(128) not null
> ,[description] nvarchar(256) null
> ,created_by nvarchar(50) null
> ,created_date datetime
> ,updated_by nvarchar(50) null
> ,updated_date datetime null
> ,is_deleted bit default 0 not null
> )
> ```
>
> There is supposed to be this table as well:
>
> ```sql
> create table [application_group_applications]
> (
> id nvarchar(50) not null PRIMARY KEY
> ,application_group_id nvarchar(50) not null
> ,application_id nvarchar(50) not null
> )
> ```
>
> Any time an application in `application-list.jsx` is assigned an application group (system), the pairing is saved as an entry in `[application_group_applications]`. Otherwise, the entry is removed when the application is not assigned a system. An application is still allowed not to have an application group / system.

**State before the change**
- `application_group` had a leftover `application_id nvarchar(50) not null` column that no code wrote. Every group insert failed on a database built from the master script.
- Membership was stored as a column on the application: `applications.application_group_id` (nullable FK).

---

## 3. Decisions Made During Implementation

| Question | Decision | Reason |
|---|---|---|
| Keep `applications.application_group_id` alongside the new table? | **Drop it.** Existing assignments are copied into `application_group_applications` first. | One source of truth. Keeping both would let them drift apart. |
| One system per application, or many? | **One.** `UNIQUE(application_id)` plus FKs to `applications` and `application_group`, added on top of the requested DDL. | The Applications form has a single System dropdown. The database enforces the same rule. |
| Change the API / dashboard contract? | **No.** `ApplicationModel.Application_Group_Id` stays, but is *read* through a join and *written* through the junction table. | The React views, alert dispatcher and group-exception logic keep working unchanged. |

---

## 4. Database

### 4.1 Final DDL (`Database/component_monitoring.sql`)

```sql
--lookup table of systems, maintained by the Application Groups page
create table [application_group]
(
id nvarchar(50) not null PRIMARY KEY
,[application_group_name] nvarchar(128) not null
,[description] nvarchar(256) null
,created_by nvarchar(50) null
,created_date datetime
,updated_by nvarchar(50) null
,updated_date datetime null
,is_deleted bit default 0 not null
)

--which system an application belongs to; no row means the application is standalone
create table [application_group_applications]
(
id nvarchar(50) not null PRIMARY KEY
,application_group_id nvarchar(50) not null
,application_id nvarchar(50) not null
,CONSTRAINT [UQ_application_group_applications_application] UNIQUE ([application_id]) -- one system per application
,CONSTRAINT [FK_application_group_applications_application_group] FOREIGN KEY ([application_group_id]) REFERENCES [dbo].[application_group] ([id])
,CONSTRAINT [FK_application_group_applications_applications] FOREIGN KEY ([application_id]) REFERENCES [dbo].[applications] ([id])
)
```

The master script no longer contains:
- `ALTER TABLE applications ADD application_group_id`
- the FK `FK_applications_application_group`

### 4.2 Migration for existing databases (`Database/migrations/002_application_group_applications.sql`)

It is idempotent: each step is guarded and in its own `GO` batch. Statements that reference a column that may already be dropped run through `sp_executesql`, so a second run still compiles.

1. **`application_group.application_id`:** if it exists, drop every FK on it (looked up through `sys.foreign_key_columns`), then drop the column.
2. **`application_group_applications`:** if missing, create it with the unique constraint and the two FKs.
3. **`applications.application_group_id`:** if it exists:
   - Backfill: `INSERT … SELECT LOWER(NEWID()), application_group_id, id FROM applications WHERE application_group_id IS NOT NULL AND NOT EXISTS (pairing)`.
   - Drop `FK_applications_application_group`.
   - Drop the column.

---

## 5. Libraries (`ModelsLibrary` & `DAL`)

### 5.1 Models
- **New** `Libraries/ModelsLibrary/Models/ApplicationGroupApplicationModel.cs` with `Id`, `Application_Group_Id`, `Application_Id`.
- **`ApplicationModel.Application_Group_Id`:** unchanged property. Its comment now states that it is read from and written to `application_group_applications` (null = standalone), and is not a column of `applications`.

### 5.2 New repository
`Libraries/DAL/Interfaces/IApplicationGroupApplicationRepository.cs` and `Libraries/DAL/Repositories/ApplicationGroupApplicationRepository.cs`, built on `DapperHelper<ApplicationGroupApplicationModel>`:

| Method | SQL |
|---|---|
| `GetByApplicationId(applicationId)` | `SELECT id, application_group_id, application_id … WHERE application_id = @applicationId` |
| `Insert(model)` | `INSERT INTO application_group_applications (id, application_group_id, application_id)` |
| `Update(model)` | `UPDATE … SET application_group_id = @Application_Group_Id WHERE id = @Id` (the row id stays stable) |
| `DeleteByApplicationId(applicationId)` | `DELETE FROM application_group_applications WHERE application_id = @applicationId` |

### 5.3 `ApplicationRepository` changes
- **Reads.** `Get`, `GetAll` and `GetPendingAlerts` read membership through a join and select `aga.application_group_id`:
  ```sql
  left join application_group_applications aga on aga.application_id = a.id
  left join application_group g on g.id = aga.application_group_id
  ```
- **Filters.** The group-exception (silencing) check and the `includeInactiveGroups` filter use `aga.application_group_id`:
  ```sql
  e.application_id = a.id or (e.application_group_id is not null and e.application_group_id = aga.application_group_id)
  ```
  ```sql
  (@includeInactiveGroups=1 or aga.application_group_id is null or g.is_deleted=0)
  ```
- **Writes.** `Insert` and `Update` no longer write any group column.
  - Side effect: the health checker calls `Update(app)` on every run, and that can no longer change an application's system.

All new files are listed in their `.csproj` with `<Compile Include>`, in alphabetical order.

---

## 6. API (`ComponentsMonitoringAPI`)

`BLL/Services/ApplicationService.cs`:
- `Create(model)` → `_repo.Insert(model)`, then `SaveGroupMembership(model)`. Database child rows are saved as before.
- `Update(model)` → `_repo.Update(model)`, then `SaveGroupMembership(model)`.
- **`SaveGroupMembership(model)`** is the only code that writes membership:

| Selected system | Existing pairing | Action |
|---|---|---|
| none (`Application_Group_Id` null/empty) | exists | `DeleteByApplicationId` |
| none | none | nothing |
| a group | none | `Insert` with `Id = Guid.NewGuid().ToString()` |
| a group | a different group | `Update` (same row, new `application_group_id`) |
| a group | the same group | nothing |

- `ApplicationController` already turns an empty `Application_Group_Id` into `null` before calling the service.
- Soft-deleting an application or a group **keeps** the pairing, so "Include Inactive Applications Group" on the Applications page keeps working.
- The group CRUD (`ApplicationGroupController` / `ApplicationGroupService` / `ApplicationGroupRepository`) needed no change. It never wrote `application_id`.

---

## 7. Unchanged Components (and Why)

| Component | Why no change is needed |
|---|---|
| `application-list.jsx`, `application-exception.jsx`, `application-group.jsx` | They only send and receive `Application_Group_Id` through the API, which keeps the same shape. |
| `ComponentsMonitoringAlertDispatcher` (`AlertDispatcherService`) | It groups digests by `Application_Group_Id` from `GetPendingAlerts`, which now comes from the join. |
| `ApplicationExceptionRepository` / `ApplicationExceptionService` | Group exceptions use `application_exceptions.application_group_id`, a separate column that stays. |
| Health checker (`ComponentMonitoringService`) | It never reads or writes membership directly, and silencing comes from `ApplicationRepository.GetAll`. |

---

## 8. Verification & Rules Checklist

**Verification**
1. `msbuild ComponentsMonitoring.sln /t:Rebuild` gives 0 errors and 0 warnings. This was verified when the change was made.
2. Run `002_application_group_applications.sql` on the dev database **twice**; the second run must be a no-op. Then confirm:
   - `application_group` has no `application_id` column.
   - `applications` has no `application_group_id` column.
   - Every application that previously had a group has exactly one row in `application_group_applications`.
3. **Application Groups page:** creating a group succeeds. It previously failed because of the `NOT NULL application_id` column.
4. **Applications page:**
   - Assigning a system to an app inserts one row.
   - Changing the system updates the same row.
   - Choosing "None (Standalone)" deletes the row.
5. **Applications page:** group sections, HEALTHY/DEGRADED badges and "Silence System" behave as before.
6. Run `ComponentsMonitoring.exe -web`. Pairings are unchanged afterwards, and apps in a silenced group are still skipped.

**Rules checklist**
- [x] New `.cs` files are added to their `.csproj` with `<Compile Include="…" />`, in alphabetical order.
- [x] No `async/await` in the BLL, DAL, or controllers.
- [x] Model properties match DB columns in `Pascal_Snake_Case` (`Application_Group_Id`, `Application_Id`).
- [x] Explicit column lists and parameterized SQL only. No `SELECT *`.
- [x] `CLAUDE.md` is updated (§7, §8 glossary "Application group (system)", §9).
