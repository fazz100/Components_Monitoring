# CLAUDE.md — Components Monitoring Solution

> **Instructions for Claude (read first, every session)**
>
> 1. **Read this file in full before generating or modifying any code** in this repository.
> 2. **These rules are the gold standard.** New code must look like it was written by the same team: same layering, folder placement, naming, and C# feature set. When this file and a nearby file disagree, follow this file. When this file is silent, copy the closest existing example.
> 3. **Maintain this file.** If you add a project, a layer, a cross-cutting pattern, a config key family, or deliberately change a convention, update the relevant section here in the same change.
> 4. **Do not "modernize" opportunistically.** No DI containers, async rewrites, EF Core, SDK-style csproj, AutoMapper, or newer C# syntax unless the user explicitly asks for it.
> 5. Section 9 lists known defects. Do **not** copy them into new code, and do not fix them silently in unrelated changes. Mention them to the user if they affect the task.

---

## 1. What this system does

An internal tool that monitors the health of DG3 production components: **Windows scheduled tasks, Windows services, and websites**. It is made up of:

- A SQL Server database (`component_monitoring`) holding the registered applications, their databases, silencing "exceptions", users, and sessions.
- An ASP.NET Web API that the dashboard calls for CRUD and auth.
- A React dashboard (`Websites/components-monitoring-dashboard`).
- Console executables run by Windows Task Scheduler. They check each component and email failures.

---

## 2. Solution architecture & projects

Solution file: `ComponentsMonitoring.sln`. Solution folders: `API`, `Applications/Scheduled Tasks`, `Applications/Console Applications`, `Libraries`, `Websites`.

| Project | Path | Type | Role |
|---|---|---|---|
| **ComponentsMonitoringAPI** | `API/ComponentsMonitoringAPI` | ASP.NET Web API 2 (+ MVC 5 shell, HelpPage area) | REST API for the dashboard. Hosts controllers and the **BLL** (services). |
| **DAL** | `Libraries/DAL` | Class library | Data access with repositories over Dapper (one repository uses raw ADO.NET). |
| **ModelsLibrary** | `Libraries/ModelsLibrary` | Class library | Shared models (entities and API DTOs share one type), enums, custom exceptions, static helpers. Referenced by every other project. |
| **ComponentsMonitoring** | `Applications/Scheduled Tasks/ComponentsMonitoring` | Console exe (scheduled task) | Health checker. Args: `-schtask`, `-winsvc`, `-web`. Sends HTML failure emails. |
| **ComponentsMonitoringExceptionsUpdater** | `Applications/Scheduled Tasks/ComponentsMonitoringExceptionsUpdater` | Console exe (scheduled task) | Removes silencing exceptions older than `days_silence_duration` (default 7). |
| **CreateAdminUser** | `Applications/Console Applications/CreateAdminUser` | Console exe (one-off) | Seeds the first admin user from `App.config`. |
| **DPAPIEncryptor** | `Applications/Console Applications/DPAPIEncryptor` | Console exe (one-off) | DPAPI-encrypts the password pepper for config files. |
| *HelpersLibrary* | `Libraries/HelpersLibrary` | Class library, **not in the .sln** | Abandoned stub (`Class1.cs`, an unreferenced `MailerHelper.cs`). Do not add code here. |
| *components-monitoring-dashboard* | `Websites/components-monitoring-dashboard` | React 19 + Vite 7 SPA (not in the .sln) | Front end. |

**Dependency direction (must stay acyclic):**

```
ComponentsMonitoringAPI ─┐
ComponentsMonitoring  ───┼──► DAL ──► ModelsLibrary
ExceptionsUpdater  ──────┤
CreateAdminUser  ────────┘          (all also reference ModelsLibrary directly)
DPAPIEncryptor: standalone
```

- `ModelsLibrary` references no other project.
- `DAL` references only `ModelsLibrary`.
- Hosts (the API and the exes) reference `DAL` and `ModelsLibrary`. Hosts never reference each other.

There are **no test projects** in this solution.

### Target framework & key dependencies

- **All C# projects: .NET Framework 4.7.2** (`<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>`), in **classic, non-SDK-style csproj** format with **`packages.config`** NuGet restore into `/packages` (git-ignored).
- No `<LangVersion>` is set, so the compiler default for net472 applies: **C# 7.3**.

| Package | Version | Used by |
|---|---|---|
| Dapper | 2.1.72 | DAL, ComponentsMonitoring |
| Microsoft.AspNet.WebApi (Core/WebHost/Client) | 5.3.0 / 6.0.0 client | API |
| Microsoft.AspNet.WebApi.Cors / Microsoft.AspNet.Cors | 5.3.0 | API |
| Microsoft.AspNet.Mvc / Razor / WebPages / Web.Optimization | 5.3.0 / 3.3.0 / 3.3.0 / 1.1.3 | API (template shell + HelpPage) |
| Newtonsoft.Json | 13.0.1 | API (default Web API serializer) |
| Konscious.Security.Cryptography.Argon2 | 1.3.1 | API, CreateAdminUser (password hashing) |
| TaskScheduler (Microsoft.Win32.TaskScheduler) | 2.12.2 | ComponentsMonitoring |
| **Dg3.CommonLibraries.LogWriter** | in-house DLL in `/dependencies` | API, both scheduled tasks |
| **Dg3.CommonLibraries.UtilityMethods** | in-house DLL in `/dependencies` | ComponentsMonitoring |

Framework assemblies in use: `System.Configuration`, `System.Data.SqlClient`, `System.Security` (DPAPI `ProtectedData`), `System.ServiceProcess`, `System.Net.Mail`.

Front end: `react` ^19.1, `react-dom`, `react-router-dom` ^7, `@fortawesome/*`, `vite` ^7, `eslint` ^9. Plain JavaScript/JSX (no TypeScript).

### Build & run

```powershell
nuget restore ComponentsMonitoring.sln
msbuild ComponentsMonitoring.sln /p:Configuration=Release      # or build in Visual Studio
# Front end
cd Websites/components-monitoring-dashboard; npm install; npm run dev   # also: npm run build / npm run lint
```

- `Database/component_monitoring.sql` is the full schema plus seed script. Update it whenever you change the schema.
- `Deployment/` holds **committed build/publish outputs** (exes, DLLs, configs, built SPA). Never hand-edit files there. It is regenerated from builds.

---

## 3. Directory & folder structure

### ComponentsMonitoringAPI

```
API/ComponentsMonitoringAPI/
├── App_Start/          WebApiConfig (attribute routes only), FilterConfig, RouteConfig, BundleConfig
├── Areas/HelpPage/     Auto-generated Web API help page. Do not modify.
├── BLL/
│   ├── Interfaces/     I{Name}Service.cs
│   └── Services/       {Name}Service.cs
├── Controllers/        {Entity}Controller.cs : CustomApiController
├── Helpers/            Host-specific static helpers (DpapiHelper)
├── Providers/          OWIN OAuth providers (ApplicationOAuthProvider, ApplicationRefreshTokenProvider)
├── Content/ Scripts/ Views/   MVC template leftovers. Do not extend.
├── Global.asax(.cs)
├── Startup.cs          OWIN pipeline: CORS -> OAuth -> Web API
└── Web.config          appSettings + connectionStrings
```

### DAL

```
Libraries/DAL/
├── Helpers/        DapperHelper<T> (config connection string), DapperHelper2<T> (explicit connection string)
├── Interfaces/     I{Entity}Repository.cs
└── Repositories/   {Entity}Repository.cs
```

### ModelsLibrary

```
Libraries/ModelsLibrary/
├── Helpers/                    Static utility classes shared across hosts ({Purpose}Helper.cs)
└── Models/
    ├── {Entity}Model.cs        DB-mapped entity/DTO (one per table)
    ├── API/                    API-only shapes: APIResponseModel<T>, APIExceptionModel, LogoutRequestModel, ReponseCodeModel
    ├── CustomExceptions/       {Name}Exception : Exception
    └── Enums/                  Enums (ApiResponseCode, WorkingStatus)
```

### Scheduled task / console exes

```
Applications/Scheduled Tasks/{Project}/
├── BLL/Interfaces/I{Name}Service.cs
├── BLL/Services/{Name}Service.cs
├── Helpers/            Exe-specific helpers (e.g. MailerHelper)
├── Program.cs          Arg parsing + top-level try/catch only
└── App.config
```

One-off console tools (`Applications/Console Applications/*`) are flat: `Program.cs` plus a helper file.

### Front end

```
Websites/components-monitoring-dashboard/
├── public/config.json  Runtime config (API_BASE_URL, API_AUTH_TOKEN, DAYS_SILENCE_DURATION)
└── src/
    ├── api/            {resource}-api.js. One module per API controller; exports async functions.
    ├── components/     Shared UI components (header.jsx, protected-route.jsx)
    ├── config/         config.js. getConfig() fetches /config.json at runtime.
    ├── helpers/        Non-UI helpers (auth-token-helper.js)
    ├── views/          Routed pages (application-list.jsx, application-exception.jsx, user.jsx, login.jsx)
    ├── App.jsx         BrowserRouter + routes
    └── App.css / index.css
```

### File placement rules (follow exactly)

| You are adding… | Put it in… | Namespace |
|---|---|---|
| API endpoint | `API/ComponentsMonitoringAPI/Controllers/{Entity}Controller.cs` | `ComponentsMonitoringAPI.Controllers` |
| Business service + its interface | `…API/BLL/Services/{Name}Service.cs` + `…API/BLL/Interfaces/I{Name}Service.cs` | `ComponentsMonitoringAPI.BLL.Services` / `.BLL.Interfaces` |
| OAuth provider / OWIN middleware config | `…API/Providers/` (pipeline itself only in `…API/Startup.cs`) | `ComponentsMonitoringAPI.Providers` |
| API-only helper | `…API/Helpers/` | `ComponentsMonitoringAPI.Helpers` |
| Repository + interface | `Libraries/DAL/Repositories/{Entity}Repository.cs` + `Libraries/DAL/Interfaces/I{Entity}Repository.cs` | `DAL.Repositories` / `DAL.Interfaces` |
| DB entity / request-response model | `Libraries/ModelsLibrary/Models/{Entity}Model.cs` | `ModelsLibrary.Models` |
| API envelope / API-only DTO | `Libraries/ModelsLibrary/Models/API/` | `ModelsLibrary.Models.API` |
| Enum | `Libraries/ModelsLibrary/Models/Enums/` | `ModelsLibrary.Models.Enums` |
| Custom exception | `Libraries/ModelsLibrary/Models/CustomExceptions/` | `ModelsLibrary.Models.CustomExceptions` |
| Cross-project static helper | `Libraries/ModelsLibrary/Helpers/` | `ModelsLibrary.Helpers` |
| New monitoring check | a method on `ComponentMonitoringService` + an arg `case` in that exe's `Program.cs` | `ComponentsMonitoring.BLL.*` |
| Schema change | `Database/component_monitoring.sql` | — |
| Front-end API call | `src/api/{resource}-api.js` | — |
| Front-end page | `src/views/{page-name}.jsx` + `<Route>` in `App.jsx` | — |

**Namespaces always mirror the folder path** under the project's root namespace.

> ⚠️ **Classic csproj:** every new `.cs` file **must** also be added as `<Compile Include="Folder\File.cs" />` in the project's `.csproj`, kept alphabetically within its `ItemGroup`. Files that are not listed are not compiled.

---

## 4. Design patterns & code structure

### 4.1 Layering

`Controller → I{Name}Service (BLL) → I{Entity}Repository (DAL) → Dapper → SQL Server`

- **Controllers** are thin. They stamp audit fields (`Id`, `Created_*`/`Updated_*`), do small input clean-up, call one service method, and return through `TryCatchWrapper`.
- **Services** hold business rules: orchestrating several repositories, hashing passwords, raising `APIExceptionModel` for business errors.
- **Repositories** hold SQL only. No business logic.
- Scheduled-task exes skip controllers: `Program.Main` → service → repositories.

### 4.2 Interfaces, implementations & "DI"

- Every service and repository has an interface: `I{Name}Service` / `I{Entity}Repository`, in the sibling `Interfaces/` folder.
- **There is no DI container.** Dependencies are typed as the interface but **constructed with `new` in a parameterless constructor**. Web API activates controllers with the default activator, so controllers must keep a public parameterless constructor.

```csharp
public class ApplicationExceptionService : IApplicationExceptionService
{
   private readonly IApplicationExceptionRepository _repository;

   public ApplicationExceptionService()
   {
      _repository = new ApplicationExceptionRepository();
   }
}
```

- Effective lifetimes: controllers are created per request, so their services and repositories are per request too. `LogWriter` is `static` per class. Scheduled-task services are created once in `Main` and held in a `static` field on `Program`.
- Do not introduce Unity/Autofac/`Microsoft.Extensions.DependencyInjection` unless the user asks.

### 4.3 Models / DTO pattern

- **One model class per table, reused as entity, request DTO, and response DTO.** No separate Request/Response DTOs, no mapping library, no mapping methods. The only API-specific shapes are in `Models/API/` (`APIResponseModel<T>`, `LogoutRequestModel`, …).
- Model classes are plain `{ get; set; }` auto-property bags with **no logic and no data annotations**.
- **Property naming is load-bearing.** DB columns are `snake_case`. Dapper maps columns to properties **case-insensitively but not underscore-insensitively**, so properties are **`Pascal_Snake_Case` matching the column**: `application_name` → `Application_Name`, `ip_address` → `IP_Address`, `created_date` → `Created_Date`. Single-word columns are plain PascalCase (`Id`, `Description`, `Token`).
- The same property names go out in JSON (Newtonsoft, no contract resolver), so the React app reads `app.Application_Name` and `res.Data`. **Renaming a model property is a breaking API change.**
- `UserModel` also has PascalCase alias properties (`FirstName` ⇄ `First_Name`, etc.) that wrap the snake-named property. Use this pattern only when an existing PascalCase name must keep working.
- Non-persisted helper properties live on the model (e.g. `UserModel.PasswordString` for incoming plaintext, `ApplicationModel.Databases` for child rows filled in by the service).
- Nullability: use `bool?`, `int?`, `DateTime?` for nullable DB columns. Reference types are plain (no NRT).
- **IDs** are `string` GUIDs from `Guid.NewGuid().ToString()` (`nvarchar(50)` PKs). Raw refresh tokens use `Guid.NewGuid().ToString("N")` and are only ever stored as a SHA-512 hash.
- **Timestamps** are always UTC: `DateTime.UtcNow`. Convert for display only, via `ModelsLibrary.Helpers.DateTimeHelper` (Eastern time).
- **Audit columns** on tables: `created_date`, `created_by`, `updated_date`, `updated_by`. Controllers set them from `CurrentUserId`.
- **Soft delete** applies to `applications` and `user` (`is_deleted` bit), and list queries filter `is_deleted=0`. Child and auxiliary tables (`application_databases`, `application_exceptions`) are hard-deleted.
- Enums persisted as `int` are stored as `int` on the model and cast at use: `app.Working_Status == (int)WorkingStatus.Working`.

### 4.4 Database access (Dapper)

- Each repository holds `private readonly DapperHelper<TModel> _dapper;`, created in its constructor. `DapperHelper<T>` reads `ConfigurationManager.ConnectionStrings["database"]`, so every host config **must define a connection string named `database`**.
- `DapperHelper<T>` API: `Get(sql, param)` → `T` (QueryFirstOrDefault), `GetAll(sql, param)` → `List<T>`, `Execute(sql, model)` for insert, `Execute(sql, object)` for update/delete.
- `DapperHelper2<T>(connectionString)` is for queries against an arbitrary connection string (e.g. `TestDatabaseConnection`).
- **SQL style:**
  - Inline verbatim strings in a local `string sql = @"…";`
  - Lowercase snake_case identifiers, `[brackets]` around reserved words (`[description]`, `[user]`, `[working_status]`).
  - Explicit column lists. Avoid `SELECT *` in new code.
  - Always parameterized: `@param` bound to an anonymous object `new { id = id }` or to the model itself (Dapper matches `@Application_Name` → property). **Never concatenate user input into SQL.**
  - Optional filters use the null-guard idiom: `(@appName is null or a.application_name like @appName + '%')`.
- Repository method names: `Get(id)`, `GetAll(filters…)`, `GetBy{Field}(value)`, `Insert(model)`, `Update(model)`, `Delete(id[, updatedBy])`, plus domain verbs where needed (`Revoke`, `RevokeByHash`, `GetActiveByHash`, `TestDatabaseConnection`). Return `void` for writes (or `bool` when the caller must know a conditional update hit a row, e.g. `RefreshTokenRepository.Revoke`), the model / `List<T>` / `null` for reads. `DapperHelper.Execute` returns the affected row count.
- No Unit of Work and no explicit transactions. Each call opens its own connection. Multi-row operations (e.g. `ApplicationService.Create` inserting child databases) loop over repository calls.
- **Do not add EF / EF Core.** For new repositories use `DapperHelper<T>`, not the raw `SqlCommand`/`SqlDataReader` style in `ResponseCodeRepository`.

### 4.5 API conventions

- Controllers inherit **`CustomApiController`** (which inherits `ApiController`) and return `IHttpActionResult`.
- **Every action body goes through `TryCatchWrapper(() => …)`**. It wraps the result in the standard envelope:

```json
{ "Code": 200, "Message": "Success", "Data": <T>, "Details": null }
```

  Write actions return `true` (or the new `Id`) from the lambda. Prefer the expression-bodied form for one-line actions:

```csharp
[HttpGet]
[Route("api/exceptions/{id}")]
public IHttpActionResult Get(string id) => TryCatchWrapper(() => _service.GetById(id));
```

- **Routing:** attribute routes only, lowercase **kebab-case**, verb-in-path style. Use `[RoutePrefix("api/{resource}")]` + `[Route("…")]` for new controllers.
- **HTTP methods: only GET and POST.** GET is for reads. POST is for every write, including create, update, soft or hard delete, and RPC actions. **Never use PUT or DELETE in new endpoints.** The PUT/DELETE endpoints in `ApplicationController` and `ApplicationExceptionController` are legacy; leave them unless asked to convert them, since the front end calls them.
  - `GET api/{resource}/all` (or `GET api/{resource}/{optionalFilters?}`) for lists, with filters as query-string params
  - `GET api/{resource}/{id}`
  - `POST api/{resource}/create` (or `save`)
  - `POST api/{resource}/update`
  - `POST api/{resource}/delete/{id}`
  - `POST api/{resource}/{action-name}` for RPC-style actions (e.g. `test-connection`)
  - Reference controller: `ApplicationGroupController`.
- Mark each action with an explicit `[HttpGet]` or `[HttpPost]`. Bind complex bodies with `[FromBody]` or a model parameter. Front-end `src/api/*` functions use `method: 'POST'` for all writes.
- **Auth is OAuth 2.0 via OWIN** (Katana 4.2.2, copied from the tested sample `MyOAuthWebApi`). Pipeline order in `Startup.cs` is fixed: **CORS → OAuth → `app.UseWebApi(GlobalConfiguration.Configuration)`**.
  - **`POST /token`** (OWIN, not a controller) takes an `x-www-form-urlencoded` body:
    - `grant_type=password` with `username`, `password`, `client_id`
    - `grant_type=refresh_token` with `refresh_token`, `client_id`

    It returns `access_token`, `refresh_token`, `expires_in`, `userName`, `userId`, `fullName`. Errors are OAuth-standard `{ error, error_description }` with HTTP 400, **not** the `APIResponseModel` envelope.
  - **Access tokens** are OWIN default bearer tokens (opaque, protected by the machine key; **not JWT**). Every server in a farm needs the same `<machineKey>`. Lifetime comes from `access_token_expiry_in_minutes`, and they cannot be revoked.
  - **Clients:**
    - `ApplicationOAuthProvider.ValidateClientAuthentication` requires a `client_id` that is in `[api_clients]` with `is_active = 1`.
    - A client with a `client_secret_hash` must also send a secret whose SHA-512 hash matches. The SPA (`components_monitoring_react_app`) is a public client with no secret.
    - To add a consumer, insert an `[api_clients]` row.
  - **Users:**
    - `GrantResourceOwnerCredentials` checks the user via `UserRepository.GetByUsername` and `UserService.VerifyPassword` (Argon2id + pepper, §4.8). Deleted users are rejected.
    - The ticket is built by `ApplicationOAuthProvider.CreateTicket`, the single place claims are defined: `NameIdentifier` = user id, `Name` = username, `FullName`.
  - **Refresh tokens** (`ApplicationRefreshTokenProvider`) are **single-use**, stored in `[refresh_tokens]` as a SHA-512 `token_hash` with a lifetime from `refresh_token_expiry_in_days`.
    - On use, the row is revoked with a conditional update (`is_revoked = 0`), so a concurrent reuse fails.
    - The ticket is then rebuilt from the current `[user]` row.
    - Logout (`POST api/auth/logout` with `{ RefreshToken }`) revokes it.
  - Protect controllers with **`[Authorize]`** (`System.Web.Http`) at class level. Do not add custom auth attributes or message handlers.
  - Read the caller with `CurrentUserId` (from `CustomApiController`). It reads `ClaimTypes.NameIdentifier` from `User.Identity`. Never parse auth headers in actions.
- **CORS** is the OWIN CORS middleware in `Startup.cs`. Its policy allows **only** the `reactAppUrl` appSetting origin, with any header and any method. Do **not** also enable Web API CORS (`config.EnableCors`) or add CORS headers by hand, because duplicate `Access-Control-Allow-Origin` headers make browsers reject the response.
- `Web.config` `<system.webServer>` must keep `runAllManagedModulesForAllRequests="true"` and the WebDAV/OPTIONS handler removals, so every request, including preflight `OPTIONS`, reaches OWIN.

### 4.6 Error handling

- **Business/expected errors:** throw `APIExceptionModel` (in `ModelsLibrary.Models.API`) with an `ApiResponseCode` enum value:

```csharp
var apiException = new APIExceptionModel();
apiException.Code = ApiResponseCode.UserExists;
throw apiException;
```

  `TryCatchWrapper` logs it at **Info**, looks up `Code`/`Message` from the `response_code` table, and returns HTTP 500 with that envelope.
  **When adding a code:** add the enum member (codes 9xx) **and** an `INSERT INTO response_code` row in `Database/component_monitoring.sql`.
- **Unexpected errors:** let them bubble up. `TryCatchWrapper` logs at **Error** and returns `{Code:500, Message:"Error", Details:<exception>}`.
- Login failure is **not** an exception. The OAuth provider calls `context.SetError("invalid_grant", …)`, and `/token` returns HTTP 400 `{ error, error_description }`. Provider code catches exceptions, logs them, and returns `server_error` without the exception text.
- Exes: `Program.Main` has a top-level `try/catch` that logs and writes to the console. Each `Process*` service method has its own try/catch so one failing component does not stop the others. It logs and emails the failure (`NotifyFailure`) and continues.
- Domain-specific exceptions go in `Models/CustomExceptions` and follow the three-constructor `[Serializable]` pattern of `InvalidWebsiteUrlException`.
- Existing code often wraps methods in `try { … } catch (Exception) { throw; }`. This is harmless but adds nothing. **New code should omit it** unless the catch logs or translates. **Never write `throw ex;`**, because it loses the stack trace. Use `throw;`.

### 4.7 Logging

- Use the in-house **`Dg3.CommonLibraries.LogWriter`** (`using Dg3.CommonLibraries;`). Declare it once per class:

```csharp
private static LogWriter LOGGER = new LogWriter();
```

- Use `LOGGER.Info(string|Exception)` for progress and expected issues, and `LOGGER.Error(Exception|string)` for failures.
- Scheduled tasks log start and end of each run (`"Starting Scheduled Task Checking"` … `"Checking Scheduled Task Complete"`) and one Info line per component checked, using string interpolation.
- **Never log secrets** (passwords, peppers, tokens, connection strings).

### 4.8 Configuration & secrets

- Read settings in place:
  - `WebConfigurationManager.AppSettings["key"]` in the API.
  - `ConfigurationManager.AppSettings["key"]` in exes and libraries.
- Parse settings with a default fallback where sensible: `!string.IsNullOrEmpty(x) ? int.Parse(x) : 7`.
- appSettings keys are **`snake_case`** (`smtp_host`, `days_silence_duration`, `encrypted_pepper`). The API's older key `reactAppUrl` and the framework key `owin:AutomaticAppStartup` are exceptions. Use snake_case for new keys.
- API OAuth keys:
  - `access_token_expiry_in_minutes` (15)
  - `refresh_token_expiry_in_days` (7)
  - `oauth_allow_insecure_http`: `true` only in the dev `Web.config`. `Web.Release.config` transforms it to `false`, so `/token` requires HTTPS in release builds.
- Secrets at rest use **DPAPI** (`DpapiHelper.Encrypt/Decrypt`, `DataProtectionScope.LocalMachine`). The encrypted pepper is produced by `DPAPIEncryptor` and stored in `encrypted_pepper`.
- **Passwords:** Argon2id (salt 16 bytes, `DegreeOfParallelism = 8`, `Iterations = 4`, `MemorySize = 65536`, 32-byte hash) over `password + pepper`. Stored as UTF-8 bytes of `"{saltB64}:{hashB64}"` in `user.password varbinary(max)`. Any new auth code must use exactly these parameters, or existing hashes will stop verifying.

### 4.9 Email (scheduled tasks)

`ComponentsMonitoring/Helpers/MailerHelper.Send(MailMessage)`:

- Static method, SMTP settings from `smtp_*` appSettings, `IsBodyHtml = true`.
- Recipients come from `mail_to`/`mail_cc`/`mail_bcc`, split on `,` or `;` via `FormatEmailAddresses`.
- Alerts are built in `ComponentMonitoringService.NotifyFailure` → `SendEmail`, which wraps the message in `<html><body><p>…</p></body></html>` and uses the subject `"[{region}] Component Monitoring Alert: {appName}"`. Line breaks in messages are `<br/>`.
- The `send_email` appSetting exists in `App.config` but **is not read by any code**. Do not assume it disables sending.

### 4.10 Front end

- **Function components** with hooks only. PascalCase component names, **kebab-case file names** (`application-list.jsx`), `export default` at the bottom.
- All HTTP goes through `src/api/*-api.js`:
  - `async` functions using **`authFetch`** from `helpers/auth-token-helper.js`, never bare `fetch`, for API calls.
  - URL built from `(await getConfig()).API_BASE_URL`.
  - `authFetch` adds `Authorization: Bearer {access_token}`. On a 401 it silently refreshes once and retries.
  - The refresh goes through `refreshAccessToken()`, which shares one in-flight request so parallel 401s don't spend the single-use refresh token twice.
  - Pass `body` as a function when it must be re-read on retry (e.g. logout sends the current refresh token).
  - Results pass through `handleApiResponse()`, which throws on non-OK and redirects to `/` on 401.
- Token requests go through `requestToken(params)`, which sends a form-encoded `POST {API_BASE_URL without trailing slash}/token` and adds `client_id` from config `OAUTH_CLIENT_ID`. OWIN won't match `//token`.
- Callers unwrap the envelope with `res.Data` and use the server's `Pascal_Snake` property names unchanged.
- Auth state is in `localStorage`: `AuthToken`/`token` (access token), `RefreshToken`, `TokenExpiry`, `username`, `userId`, `fullName`. `saveTokens()` writes the token keys.
- Runtime config comes from `public/config.json`. Do not use `import.meta.env` for deploy-time values.
- Styling is plain CSS in `App.css`/`index.css`. Icons are FontAwesome. No UI framework or CSS-in-JS.

---

## 5. Enforced coding conventions (C#)

### 5.1 Naming

| Element | Rule | Example |
|---|---|---|
| Interface | `I` + PascalCase, same name as its implementation | `IApplicationService` |
| Service | `{Domain}Service` | `ApplicationExceptionService` |
| Repository | `{Entity}Repository` | `RefreshTokenRepository` |
| Controller | `{Entity}Controller` (singular) | `ApplicationDatabaseController` |
| Model | `{Entity}Model` (singular, except the existing `ApplicationExceptionsModel`) | `ApplicationDatabaseModel` |
| Helper | `{Purpose}Helper`, usually `static class` | `DpapiHelper`, `HashHelper` |
| OAuth provider | `Application{Purpose}Provider` | `ApplicationRefreshTokenProvider` |
| Exception | `{Name}Exception` | `InvalidWebsiteUrlException` |
| Enum / members | PascalCase / PascalCase | `WorkingStatus.KnownIssue` |
| Methods | PascalCase, verb-first (`Get…`, `Process…`, `Check…`, `Notify…`) | `ProcessScheduledTasks` |
| Private instance fields | `_camelCase`, `readonly` when assigned only in ctor | `_repository`, `_userService` |
| Static logger | `LOGGER` | `private static LogWriter LOGGER` |
| Static constants/config | `UPPER_SNAKE_CASE` | `TIMEZONE_ID` |
| Locals / parameters | camelCase | `userId`, `appName` |
| Model properties | `Pascal_Snake_Case` matching DB column (see 4.3) | `Created_By` |
| **Async suffix** | n/a. The codebase is **fully synchronous**. | — |

- Keep the existing misspelled type `ReponseCodeModel`. Do not rename it.
- Abbreviation casing follows existing types: `API` in `APIResponseModel`/`APIExceptionModel`, `Api` in `ApiResponseCode`/`CustomApiController`. Copy whichever name already exists.

### 5.2 Language version & feature use (C# 7.3 / .NET Framework 4.7.2)

**Available and used. Prefer these:**

- Expression-bodied members for one-line methods and properties (`public void Update(ApplicationModel m) => _repo.Update(m);`).
- String interpolation `$"…"`, null-conditional `?.`, `??`, `nameof`, `var` for obvious types.
- Object initializers (`new RefreshTokenModel { … }`), `using` statements with braces, `out var`, tuples / `Tuple<…>`.
- `default` literal.

**Not available. Do not use:**

- File-scoped namespaces. Use **block-scoped `namespace X { … }`**.
- Primary constructors, records, `init` accessors, nullable reference types (`string?`, `#nullable`), switch expressions, `using var` declarations, target-typed `new()`, global usings, raw string literals, `required` members.

**Structure:**

- One public type per file. File name = type name.
- Explicit access modifiers on types and members. Classes are `public` unless they are `Program`.
- `using` directives go at the top, outside the namespace. Project namespaces appear before `System.*` as VS sorts them, but either order is acceptable. Remove usings you add that go unused.
- Synchronous APIs only. Do not introduce `async/await` into Web API actions, services, or repositories unless the whole call chain is converted at the user's request. The React front end is async (`fetch`).
- Braces on their own line (Allman) for types, methods, and multi-line blocks. Single-line `if (…) return …;` without braces is accepted for guard clauses.
- Comments are sparse and practical (`//check if username already exists`, `// Cleanup`). Do not add XML doc comments unless the surrounding file has them.

### 5.3 Formatting

- Files are UTF-8 **with BOM**, CRLF line endings (Visual Studio defaults).
- Indentation uses **3-column** indents. Existing files mix tabs (tab width 3) and 3-space indents. **Match the file you are editing.** For new files use **3 spaces**, as in `ApplicationExceptionService.cs` and `ApplicationExceptionRepository.cs`.
- Front end: 4-space indent in `src/api/*` and views, 2-space in `main.jsx`/`App.jsx`/`header.jsx`. Match the file. Single quotes, semicolons in most files.

---

## 6. Checklist: adding a new CRUD feature end-to-end

1. **Schema:** add the table (snake_case, `id nvarchar(50)` PK, audit columns, `is_deleted` if it is soft-deletable) to `Database/component_monitoring.sql`.
2. **Model:** add `Libraries/ModelsLibrary/Models/{Entity}Model.cs` with `Pascal_Snake` properties matching the columns. Add a `<Compile Include>` entry.
3. **Repository:** add `DAL/Interfaces/I{Entity}Repository.cs` and `DAL/Repositories/{Entity}Repository.cs` using `DapperHelper<{Entity}Model>` and parameterized SQL. Add `<Compile Include>` entries.
4. **Service:** add `API/.../BLL/Interfaces/I{Entity}Service.cs` and `BLL/Services/{Entity}Service.cs`. Construct repositories with `new` in the constructor. Throw `APIExceptionModel` for business-rule failures. Add `<Compile Include>` entries.
5. **Controller:** add `Controllers/{Entity}Controller.cs : CustomApiController` with `[RoutePrefix("api/{kebab-resources}")]` and `[Authorize]`. Use GET for reads and POST for every write; never PUT or DELETE. Every action goes through `TryCatchWrapper`. Stamp `Id`/audit fields from `CurrentUserId` + `DateTime.UtcNow`. Add a `<Compile Include>` entry.
6. **Response codes:** if there are new business errors, add an `ApiResponseCode` member and a `response_code` seed row.
7. **Front end:** add `src/api/{resource}-api.js` (copy `application-api.js`'s `getHeaders`/`handleApiResponse` pattern), then a view in `src/views/` and a route in `App.jsx`.
8. **Config:** add any new appSettings (snake_case) to every host config that needs them, and to `Deployment/` configs only when producing a deployment.
9. **Update this CLAUDE.md** if the feature introduces a new pattern.

---

## 7. Checklist: adding a new monitoring check

1. Add `void Process{Things}()` to `IComponentMonitoringService` and implement it in `ComponentMonitoringService`:
   - Load components with `_applicationRepository.GetAll("{application_type}")`.
   - Wrap the run in try/catch and log start/end.
   - Check each item in its own try/catch so one failure does not stop the rest.
   - On failure, flip `Working_Status` to `NotWorking` **only if it is currently `Working`**, persist via `_applicationRepository.Update`, and call `NotifyFailure(appName, htmlMessage)`.
2. Add a `case "-{flag}":` to `Program.Main` and to `ShowUsage()`.
3. Respect silencing: `IApplicationRepository.GetAll(type)` defaults to `includeExceptions = false`, so apps with an active exception are skipped automatically. Do not pass `true` from a checker. (The API's `IApplicationService` defaults to `true` because the dashboard shows everything.)
   - `GetAll` also has `includeInactiveGroups`, which **defaults to `true` in the repository** so checkers keep monitoring apps whose application group was soft-deleted. Only the API passes `false` (its default), so the dashboard hides those apps unless the user ticks "Include Inactive Applications Group".
   - `ApplicationRepository.Get` and `Update` must keep the same column set. The checker does `Get(id)` followed by `Update(app)`, so a column written by `Update` but not read by `Get` is wiped on every run.
4. Allowed `application_type` values: `"scheduled task"`, `"windows service"`, `"website"`. They are compared as literal strings, so keep the exact spelling.

---

## 8. Domain glossary

- **Application:** a monitored component (row in `applications`). `application_type` decides which checker handles it. `url_or_app_name` holds the task name, service name, or URL. `ip_address` holds the server host.
- **Working status:** `WorkingStatus` enum. `0` NotWorking, `1` Working, `2` KnownIssue, `3` NoLongerNeeded.
- **Exception (application exception):** a *silencing* record in `application_exceptions`. It suppresses alerts for an app until the ExceptionsUpdater removes it after `days_silence_duration` days. These are unrelated to .NET exceptions.
- **Application group:** a named system that applications belong to (`application_group`, soft-deleted via `is_deleted`). Membership is `applications.application_group_id` (nullable FK, `NULL` = standalone). The Applications page sorts by group by default and hides apps whose group is deleted unless "Include Inactive Applications Group" is checked. Groups are managed on the Application Groups page (`views/application-group.jsx`).
- **Application database:** a connection string associated with an app (`application_databases`) whose connectivity can be tested.
- **Refresh token:** row in `refresh_tokens` (SHA-512 `token_hash`, single-use, `is_revoked`). It replaced the old `user_session` table, which the migration script drops once you run its final step.
- **API client:** row in `api_clients`. An application allowed to request tokens from `/token` (e.g. `components_monitoring_react_app`).

---

## 9. Known defects. Do not copy into new code

These exist today. Do not replicate them in new code. Fix them only when the task is about them or the user approves.

- `DapperHelper` creates `SqlConnection`s without `using`/`Dispose`. Any new helper method must wrap the connection in `using (IDbConnection db = new SqlConnection(_connectionString)) { … }`.
- `throw ex;` in `ResponseCodeService` and `ResponseCodeRepository` loses the stack trace.
- `UserService.GetUsers` swallows exceptions and returns `null`.
- Secrets are committed: the DPAPI entropy string and SQL credentials in `App.config`.
- `ProtectedRoute` is a no-op and its usages in `App.jsx` are commented out, so the front-end routes are unguarded. The API's `[Authorize]` still enforces auth.
- `[refresh_tokens]` has no `client_id` column, so a refresh token is not bound to the client that received it. Every refresh request is still checked against `[api_clients]`.
- `ComponentsMonitoringAPI.csproj` and `ComponentsMonitoringExceptionsUpdater.csproj` reference `Dg3.CommonLibraries.LogWriter.dll` via a `bin\Debug` hint path of another project. New references to in-house DLLs should point at `/dependencies`.
- `APIExceptionModel` business errors return HTTP 500 rather than a 4xx.
- `ConfigValue` in `ModelsLibrary.Models.API` contains unrelated, unused keys (MSMQ/SFTP).
- `ExecutionLogRepository` / `ExecutionBatchRepository` are empty stubs, and `HelpersLibrary` is dead code.
- `ComponentMonitoringService.cs` ends with a large commented-out copy of an older version of the class. Leave it alone unless asked, and do not add new commented-out code blocks.
