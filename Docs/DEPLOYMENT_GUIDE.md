# Components Monitoring — Deployment Guide

Use this when deploying the dashboard, the API and the scheduled tasks to a region (DEV / PROD).

Hosting layout: one IIS site serves the React dashboard, and the API is an IIS **sub-application at `/api`** under it.

> **Golden rule for config files:** never overwrite a server's `Web.config` / `App.config` with the repo copy. The repo files contain **developer-machine values**: connection strings, the DPAPI-encrypted pepper, and log paths. Merge the *new* settings into the server's file instead.
>
> The files under `Deployment/` are committed build outputs and may be **older than the code**. Use them only as a reference for server-specific values.

---

## 1. Database

Run the migrations in order. Each script is safe to run more than once.

1. `Database/migrations/oauth_migration.sql`: creates `refresh_tokens` and `api_clients`, and seeds `components_monitoring_react_app`. Its last step, `DROP TABLE user_session`, is commented out; run it later.
2. `Database/migrations/001_application_group_features.sql`: alert columns on `applications`, group exceptions, response code 903.
3. `Database/migrations/002_application_group_applications.sql`: `application_group` becomes a lookup table, and membership moves to `application_group_applications`.

Check:

```sql
SELECT * FROM api_clients;                                          -- 'components_monitoring_react_app', is_active = 1
SELECT code, name FROM response_code WHERE code IN (901,902,903);   -- all three rows
SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('applications');
   -- includes alert_pending, failure_detected_at, failure_details; NOT application_group_id
SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('application_group');   -- no application_id
SELECT COUNT(*) FROM application_group_applications;               -- table exists
```

The API's app-pool identity (Integrated Security) needs read/write on the new tables:
- `refresh_tokens`
- `api_clients`
- `application_group_applications`

---

## 2. API (`/api` sub-application)

### 2.1 Build and copy
1. In Visual Studio, publish `ComponentsMonitoringAPI` with **FolderProfile**, configuration **Release**. The output goes to `API/ComponentsMonitoringAPI/bin/app.publish/`.
2. Copy **everything except `Web.config`** to the server's `/api` folder, and **replace the whole `bin` folder**. Several DLLs are new, so copying only the changed ones isn't enough.
3. Check that `api\bin` contains at least:
   - `Owin.dll`
   - `Microsoft.Owin.dll`, `Microsoft.Owin.Host.SystemWeb.dll`, `Microsoft.Owin.Security.dll`, `Microsoft.Owin.Security.OAuth.dll`, `Microsoft.Owin.Cors.dll`
   - `System.Web.Http.Owin.dll`, `System.Web.Cors.dll`
   - `Dg3.CommonLibraries.LogWriter.dll`, `DAL.dll`, `ModelsLibrary.dll`, `Dapper.dll`, `Konscious.Security.Cryptography.Argon2.dll`

   Without `Microsoft.Owin.Host.SystemWeb.dll`, `Startup.cs` never runs: `/token` returns 404 and every call returns 401.

### 2.2 Merge `Web.config` (edit the server's copy)

**Keep the server's own values for:**
- the `<Dg3.CommonLibraries.LogWriter>` block (log path, SMTP, recipients)
- `<connectionStrings>` → `database`
- **`encrypted_pepper`**: it is DPAPI LocalMachine-encrypted on *that* server. A value from another machine can't be decrypted there, so login fails with `server_error` and existing passwords stop verifying. To create a new one, run `DPAPIEncryptor` **on the server**.

**`<appSettings>`** must contain:

```xml
<add key="owin:AutomaticAppStartup" value="true" />
<!-- OAuth 2.0 -->
<add key="access_token_expiry_in_minutes" value="15" />
<add key="refresh_token_expiry_in_days" value="7" />
<!-- true only for regions served over plain HTTP; false (HTTPS required on /token) otherwise -->
<add key="oauth_allow_insecure_http" value="true" />
<!-- dashboard origin, no trailing slash: the only origin the OWIN CORS policy allows -->
<add key="reactAppUrl" value="http://YOUR-HOST" />
<add key="encrypted_pepper" value="(server's existing value)" />
```

- Remove `SessionExpiryInSeconds`; it's no longer used.
- **Warning:** a **Release** publish transforms `oauth_allow_insecure_http` to `false`. For an HTTP region, set it back to `true` on the server, or login fails.

**Replace these blocks with the repo's versions** from `API/ComponentsMonitoringAPI/Web.config`:

- **`<runtime>…</runtime>`.** It contains the OWIN binding redirects:
  - `Microsoft.Owin`, `Microsoft.Owin.Security`, `Microsoft.Owin.Security.OAuth` → 4.2.2.0
  - **`System.Web.Cors` → 5.3.0.0.** Microsoft.Owin.Cors references 5.0; without this redirect the API fails at startup with *Could not load file or assembly 'System.Web.Cors, Version=5.0.0.0'*, and every request returns HTTP 500.
- **`<system.webServer>…</system.webServer>`.** It routes every request, including `/token` and `OPTIONS`, through OWIN:

```xml
<system.webServer>
  <modules runAllManagedModulesForAllRequests="true">
    <remove name="WebDAVModule" />
  </modules>
  <handlers>
    <remove name="WebDAV" />
    <remove name="ExtensionlessUrlHandler-Integrated-4.0" />
    <remove name="OPTIONSVerbHandler" />
    <remove name="TRACEVerbHandler" />
    <add name="ExtensionlessUrlHandler-Integrated-4.0" path="*." verb="*" type="System.Web.Handlers.TransferRequestHandler" preCondition="integratedMode,runtimeVersionv4.0" />
  </handlers>
</system.webServer>
```

`<system.web>` (`targetFramework="4.7.2"`) and `<system.codedom>` are unchanged.

### 2.3 IIS
- **App pool:** **.NET CLR v4.0**, **Integrated** pipeline. OWIN does not run in Classic mode.
- **Recycle the app pool** after copying.

---

## 3. React dashboard (site root)

1. In `Websites/components-monitoring-dashboard`, run `npm run build`, then copy the contents of `dist/` to the site root.
2. **`config.json` on the server.** The build copies `public/config.json`, which has local-development values. Replace it with:

   ```json
   {
     "API_BASE_URL": "http://YOUR-HOST/api",
     "OAUTH_CLIENT_ID": "components_monitoring_react_app",
     "DAYS_SILENCE_DURATION": "7"
   }
   ```

   - `API_AUTH_TOKEN` no longer exists.
   - Without `OAUTH_CLIENT_ID`, `/token` returns `invalid_client`.
3. Keep the site's `web.config`. Its *React Routes* rewrite rule skips `/api`, and it requires the IIS **URL Rewrite** module.

---

## 4. Scheduled tasks (Windows Task Scheduler)

Each exe has its own `App.config`. Set the `database` connection string and the LogWriter block for the region.

| Exe | Purpose | Suggested schedule | Region-specific `appSettings` |
|---|---|---|---|
| `ComponentsMonitoring.exe -schtask` / `-winsvc` / `-web` | Health checks. Records failures (`alert_pending`); **sends no email itself.** | e.g. every 5–15 min, one task per argument | — |
| `ComponentsMonitoringAlertDispatcher.exe` | Emails pending failures, one digest per application group. **Required, or no alert emails go out.** | e.g. every 5 min | `grace_period_minutes`, `region`, `mail_from`, `mail_to`, `mail_cc`, `mail_bcc`, `smtp_*` |
| `ComponentsMonitoringExceptionsUpdater.exe` | Expires silencing exceptions (app and group) | daily | `days_silence_duration` |
| `ComponentsMonitoringAutoDiscovery.exe` | Registers new DG3 services and tasks found on target servers | daily | `target_servers`, `created_by` |

The run-as account for the checker and AutoDiscovery needs **admin rights on the monitored servers**, for remote `ServiceController` / `TaskService` / event-log access.

---

## 5. Verify and troubleshoot

1. **API starts:** browse `http://YOUR-HOST/api/api/applications` without logging in.
   - **401** = OK; OWIN and auth are running.
   - **500 / HTML error page** = startup failure. Check **Event Viewer → Windows Logs → Application** (source *ASP.NET 4.0*), or temporarily add `<customErrors mode="Off"/>` inside `<system.web>`. The usual cause is a missing `<runtime>` binding redirect (§2.2) or a missing DLL (§2.1).
2. **Login works:** `POST /api/token` returns 200 with `access_token`.

   | Response | Check |
   |---|---|
   | `invalid_client` | `api_clients` row / `OAUTH_CLIENT_ID` in `config.json` |
   | `invalid_grant` | Wrong username or password, or the user is deleted |
   | `server_error` | `encrypted_pepper` (wrong machine) or DB connection; see the API log file |
   | No token / 404 on `/token` over HTTP | `oauth_allow_insecure_http` must be `true` |

3. **Errors after login:** every API exception is returned as HTTP 500 with JSON `{ Code, Message, Details }`. Open F12 → Network → the failed request → **Response**; `Details` contains the exact exception. For example, *Invalid object name …* or *Invalid column name …* means a migration step was skipped (§1).
4. **Smoke test:** Applications, Exceptions, Application Groups and Users all load. Assigning a system to an application saves a row in `application_group_applications`.
