# AUTO_DISCOVERY_FEATURE_INSTRUCTIONS.md — Component Auto-Discovery Feature

> **Target Execution Environment:** Claude / AI Coding Assistant  
> **Context:** This document defines the exact specifications for building a new background scheduled task application (`ComponentsMonitoringAutoDiscovery`) that automatically scans target servers for Windows Services and Scheduled Tasks containing any case-insensitive variation of `dg3` (`[DdGg]3`) and registers them into the monitoring system. All architectural rules, conventions, and constraints in `CLAUDE.md` must be followed strictly (.NET Framework 4.7.2, C# 7.3, Dapper ORM, classic `.csproj` formatting, synchronous methods).

---

## 1. Overview & Objectives

In production environments, Windows services and scheduled tasks are deployed or updated frequently. To reduce manual administrative overhead, this feature introduces an automated scanner:
1. **Target Server Configuration:** Reads a comma-separated list of target server hostnames from `App.config`.
2. **Remote Inspection:** Connects to each server remotely to enumerate:
   - Windows Services whose name or display name contains **`dg3`** case-insensitively (`dg3`, `DG3`, `Dg3`, etc.).
   - Scheduled Tasks whose name or path contains **`dg3`** case-insensitively.
3. **Database Deduplication & Registration:** 
   - Checks if each discovered component already exists in the `[applications]` table (matching by `url_or_app_name` and `ip_address`).
   - If not found, automatically registers it as a new application component.
4. **Standalone Assignment:** Newly auto-discovered components are created as standalone entries (no initial application group / system assignment in `application_group_applications`).

> *Note on Database Schema:* The `ip_address` column in the `[applications]` table doubles as the server/host identifier (e.g., `wwwpw01.us.dg3.local`). It is an `nvarchar` field, so full hostnames and alpha-numeric strings with dashes and dots are fully supported.

---

## 2. Project Setup & Solution Integration

Create a new console application project:
* **Path:** `Applications/Scheduled Tasks/ComponentsMonitoringAutoDiscovery`
* **Target Framework:** `.NET Framework 4.7.2` (`<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>`)
* **Project Format:** Classic, non-SDK-style csproj with `packages.config` NuGet restore.
* **Dependencies:** Reference `DAL`, `ModelsLibrary`, and the in-house DLL `Dg3.CommonLibraries.LogWriter` (via `/dependencies`). Also reference NuGet packages `Microsoft.Win32.TaskScheduler` and `Dapper`.
* **Solution File:** Add the project to `ComponentsMonitoring.sln` under the solution folder `Applications/Scheduled Tasks`. Ensure every new `.cs` file is explicitly included via `<Compile Include="..." />` in alphabetical order inside its `.csproj`.

---

## 3. Configuration (`App.config`)

Add the required connection strings and appSettings to the project's `App.config`:

```xml
<appSettings>
   <!-- Comma-separated list of target servers/hosts to scan -->
   <add key="target_servers" value="wwwpw01.us.dg3.local,apppw01.us.dg3.local" />
</appSettings>
<connectionStrings>
   <add name="database" connectionString="Server=YOUR_SERVER;Database=component_monitoring;Trusted_Connection=True;" providerName="System.Data.SqlClient" />
</connectionStrings>