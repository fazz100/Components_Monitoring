# APPLICATION_GROUP_FEATURES_INSTRUCTIONS.md — Application Group Functional Enhancements

> **Target Execution Environment:** Claude / AI Coding Assistant  
> **Context:** This document defines the exact changes needed to build out functional features for existing Application Groups. All rules, conventions, and constraints in `CLAUDE.md` must be followed strictly (.NET Framework 4.7.2, C# 7.3, Dapper ORM, classic `.csproj` formatting, synchronous methods, `Pascal_Snake_Case` property mapping).

---

## 1. Overview & Objectives

The `application_group` entity and its corresponding CRUD operations in the API and React dashboard currently exist only as metadata tags. This update transforms Application Groups into functional operational units across three key areas:

1. **Group-Level Batch Email Alerts (Digest / Pending Alert Pattern):**
   - Decouple health failure detection from immediate email dispatch in the health checker (`ComponentsMonitoring`).
   - Store failure states with an `alert_pending` flag and timestamp.
   - Introduce a new lightweight background scheduled task (`ComponentsMonitoringAlertDispatcher`) that aggregates pending alerts, groups them by `application_group_id`, and sends a single consolidated HTML digest email per system.
2. **"Silence Entire System" via Group-Level Exceptions:**
   - Extend the exception/muting engine (`application_exceptions`) to support muting an entire `application_group_id` in addition to individual `application_id` records.
   - Update repository queries so active group exceptions automatically silence all member applications from checks and notifications.
3. **Group-Level Health Rollup Status on Dashboard:**
   - Update the React dashboard (`application-list.jsx`) to display system summary headers with dynamic health indicators (`HEALTHY` vs. `DEGRADED`).
   - Add a direct "Silence Entire System" quick-action button on group headers that triggers group-wide exception creation.

---

## 2. Database Migration Script

Create a new migration script `Database/Migrations/001_application_group_features.sql` (and update master schema `Database/component_monitoring.sql`):

```sql
-- 1. Add alert pending and tracking columns to applications table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[applications]') AND name = 'alert_pending')
BEGIN
    ALTER TABLE [dbo].[applications] ADD [alert_pending] BIT NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[applications]') AND name = 'failure_detected_at')
BEGIN
    ALTER TABLE [dbo].[applications] ADD [failure_detected_at] DATETIME NULL;
END;

-- 2. Support Group-Level Exceptions in application_exceptions table
-- Allow application_id to be NULL for group-wide silencing
ALTER TABLE [dbo].[application_exceptions] ALTER COLUMN [application_id] NVARCHAR(50) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[application_exceptions]') AND name = 'application_group_id')
BEGIN
    ALTER TABLE [dbo].[application_exceptions] ADD [application_group_id] NVARCHAR(50) NULL;
    
    ALTER TABLE [dbo].[application_exceptions] 
    ADD CONSTRAINT [FK_application_exceptions_application_group] 
    FOREIGN KEY ([application_group_id]) REFERENCES [dbo].[application_group] ([id]);
END;
```

---

## 3. Libraries Update (`Libraries/ModelsLibrary` & `Libraries/DAL`)

### 3.1 Model Updates

1. **`Libraries/ModelsLibrary/Models/ApplicationModel.cs`**:
   - Add properties matching new DB columns:
     ```csharp
     public bool Alert_Pending { get; set; }
     public DateTime? Failure_Detected_At { get; set; }
     ```

2. **`Libraries/ModelsLibrary/Models/ApplicationExceptionModel.cs`**:
   - Change `Application_Id` type to `string` (nullable).
   - Add `public string Application_Group_Id { get; set; }`.
   - Add non-persisted property `public string Group_Name { get; set; }` for UI/reporting mapping.

### 3.2 Data Access Layer (`Libraries/DAL`)

1. **`ApplicationRepository.cs`**:
   - Update `Get` and `GetAll` SQL queries to map `alert_pending` and `failure_detected_at`.
   - Update `Update` SQL method to save `alert_pending` and `failure_detected_at`. Ensure existing fields remain intact during updates.
   - Add method `List<ApplicationModel> GetPendingAlerts(int gracePeriodMinutes)`:
     ```sql
     SELECT a.*, g.group_name AS Group_Name
     FROM [applications] a
     LEFT JOIN [application_group] g ON a.application_group_id = g.id
     WHERE a.working_status = 0 
       AND a.alert_pending = 1 
       AND a.is_deleted = 0
       AND (a.failure_detected_at IS NULL OR a.failure_detected_at <= DATEADD(minute, -@gracePeriodMinutes, GETUTCDATE()))
     ```
   - Add method `void ClearAlertPendingFlags(IEnumerable<string> applicationIds)`:
     ```sql
     UPDATE [applications] 
     SET alert_pending = 0, updated_date = GETUTCDATE() 
     WHERE id IN @ids
     ```

2. **`ApplicationExceptionRepository.cs`**:
   - Update `Insert` method to allow inserting exceptions where `application_id` is null and `application_group_id` is populated.
   - Update filtering logic so active exceptions account for BOTH direct application exceptions AND group exceptions:
     ```sql
     WHERE NOT EXISTS (
         SELECT 1 FROM application_exceptions e 
         WHERE (e.application_id = a.id OR (e.application_group_id = a.application_group_id AND e.application_group_id IS NOT NULL))
           AND GETUTCDATE() BETWEEN e.start_date AND e.end_date
     )
     ```

---

## 4. Health Checker Refactoring (`ComponentsMonitoring`)

Modify `Applications/Scheduled Tasks/ComponentsMonitoring/BLL/Services/ComponentMonitoringService.cs`:

1. Locate component health failure logic (`ProcessWebsites`, `ProcessScheduledTasks`, `ProcessWindowsServices`).
2. When a health check fails:
   - If `Working_Status` is currently `Working` (1), transition to `NotWorking` (0).
   - Set `app.Alert_Pending = true;`
   - Set `app.Failure_Detected_At = DateTime.UtcNow;`
   - Call `_applicationRepository.Update(app);`
   - **Remove/bypass direct call to `NotifyFailure()`** inside the monitoring run loop. Log the state change via `LOGGER.Info(...)`.

---

## 5. New Scheduled Task: `ComponentsMonitoringAlertDispatcher`

Create a new console application executable for cross-category batch emailing.

### 5.1 Project Setup
- **Path:** `Applications/Scheduled Tasks/ComponentsMonitoringAlertDispatcher`
- **Target:** `.NET Framework 4.7.2`, classic `.csproj`, `packages.config`
- **Dependencies:** Reference `DAL`, `ModelsLibrary`, `Dg3.CommonLibraries.LogWriter`.
- **Solution:** Add project to `ComponentsMonitoring.sln` under solution folder `Applications/Scheduled Tasks`.

### 5.2 Implementation Details (`Program.cs` & `AlertDispatcherService.cs`)
- **App.config:** Add `grace_period_minutes` (default: `2`), along with standard `smtp_*` settings.
- **Service Workflow:**
  1. Read `grace_period_minutes` from configuration.
  2. Query pending alerts using `_applicationRepository.GetPendingAlerts(gracePeriodMinutes)`.
  3. If no pending alerts exist, log exit cleanly.
  4. Group returned applications by `Application_Group_Id` (group ungrouped/standalone apps under a fallback label `"Individual Applications"`).
  5. For each group:
     - Construct a consolidated HTML table email containing:
       - Application Name (`Application_Name`)
       - Component Type (`Application_Type`: website, scheduled task, or windows service)
       - Host Server (`IP_Address`)
       - Detection Time (`Failure_Detected_At`)
     - Subject format: `[{region}] Component Monitoring Alert: Group Alert - {GroupName} ({Count} Components Down)`
     - Send via `MailerHelper.Send()`.
  6. Call `_applicationRepository.ClearAlertPendingFlags(processedIds)` to mark alerts as dispatched.
  7. Wrap entire execution in try-catch and log through `LogWriter`.

---

## 6. Web API Updates (`ComponentsMonitoringAPI`)

### 6.1 `ApplicationExceptionController.cs` & Service
- Update `POST api/application-exception/create` (or add `POST api/application-exception/create-group-exception`) to handle payloads with `Application_Group_Id`.
- Ensure server validation requires either `Application_Id` OR `Application_Group_Id` to be provided.
- Stamp standard audit fields (`Created_By`, `Created_Date`).

---

## 7. Dashboard UI Updates (`components-monitoring-dashboard`)

### 7.1 Group Health Rollup View (`src/views/application-list.jsx`)
- Organize application cards under Application Group containers.
- Render a Group Header Bar for each system group with:
  - System Group Name & Description.
  - **Health Badge:**
    - `HEALTHY` (Green badge) if all member components are `Working`.
    - `DEGRADED` (Red/Warning badge) if 1 or more member components are `NotWorking`.
  - **Action Button:** "Silence System" button opening the exception modal pre-filled with the current `application_group_id`.
  - Expand/Collapse toggle for hiding/showing group components.

### 7.2 Group Exception Modal (`src/views/application-exception.jsx` / Components)
- Enhance the exception creation form to support two target modes:
  - **Single Component Mute** (dropdown lists applications).
  - **Entire System Group Mute** (dropdown lists application groups).
- Send the selected `application_group_id` to the API via `src/api/application-exception-api.js`.

---

## 8. Verification & Rules Checklist for AI Assistant

Before submitting code updates, verify the following:

- [ ] All new `.cs` files are added to their project `.csproj` under `<Compile Include="..." />` in alphabetical order.
- [ ] No `async/await` keywords introduced in C# BLL, DAL, or Controller layers.
- [ ] Property names strictly match DB column names using `Pascal_Snake_Case`.
- [ ] Visual Studio solution (`msbuild ComponentsMonitoring.sln`) builds with zero warnings or errors.
- [ ] React components build without ESLint errors (`npm run build`).