I need you to refactor my main solution (`ComponentsMonitoringAPI`) to use full OAuth 2.0 with OWIN and JWT Bearer tokens, and wire up my React dashboard accordingly.

I have already built and tested a working OAuth 2.0 reference implementation in another project on my machine. Please inspect the code files in that reference project and replicate its exact OWIN pipeline, CORS configuration, and OAuth provider structures into my main project.

---

### Reference Project Location & Files
Please read and reference the implementation details from my working sample project located at:
`F:\temp work files\MyOAuthWebApi`

Specifically inspect these reference files from that project:
- `Startup.cs` (OWIN pipeline and CORS setup)
- `Web.config` (CORS handlers and `runAllManagedModulesForAllRequests="true"` settings)
- `Providers/ApplicationOAuthProvider.cs` (OAuth provider logic)
- `Providers/ApplicationRefreshTokenProvider.cs` (Refresh token persistence)

---

### Main Project Architectural Requirements & Constraints (`ComponentsMonitoringAPI`)

1. **Preserve Existing `[dbo].[user]` Table Schema:**
   - Do NOT use Entity Framework or default Microsoft Identity tables.
   - Primary Key: `[id] NVARCHAR(50)` containing string GUIDs.
   - Password Storage: `[password] VARBINARY(MAX)` checked using my existing `UserService.VerifyPassword()` logic (Argon2id + DPAPI Pepper).

2. **Replace `[dbo].[user_session]` with `[dbo].[refresh_tokens]` using GUID Primary Keys:**
   - Create a SQL migration script for `[dbo].[refresh_tokens]`:
     ```sql
     CREATE TABLE [dbo].[refresh_tokens] (
         [id] NVARCHAR(50) NOT NULL PRIMARY KEY, -- String GUID
         [user_id] NVARCHAR(50) NOT NULL,        -- FK to [dbo].[user].[id]
         [token_hash] NVARCHAR(256) NOT NULL UNIQUE,
         [issued_at] DATETIME NOT NULL,
         [expires_at] DATETIME NOT NULL,
         [is_revoked] BIT NOT NULL DEFAULT 0,
         CONSTRAINT [FK_refresh_tokens_user] FOREIGN KEY ([user_id]) REFERENCES [dbo].[user] ([id])
     );
     ```

3. **Client Registration & Access Control (`[dbo].[api_clients]`):**
   - Create a SQL migration script for `[dbo].[api_clients]` to control which consumer applications are allowed to request tokens from the API:
     ```sql
     CREATE TABLE [dbo].[api_clients] (
         [id] NVARCHAR(50) NOT NULL PRIMARY KEY, -- String GUID
         [client_name] NVARCHAR(100) NOT NULL,    -- e.g., 'Components Monitoring Dashboard'
         [client_id] NVARCHAR(100) NOT NULL UNIQUE,
         [client_secret_hash] NVARCHAR(256) NULL,
         [is_active] BIT NOT NULL DEFAULT 1,
         [created_date] DATETIME NOT NULL DEFAULT GETUTCDATE()
     );

     -- Insert first client entry for the React Dashboard
     INSERT INTO [dbo].[api_clients] ([id], [client_name], [client_id], [client_secret_hash], [is_active], [created_date])
     VALUES (
         LOWER(NEWID()), 
         'Components Monitoring Dashboard', 
         'components_monitoring_react_app', 
         NULL, -- Public SPA client (no secret required)
         1, 
         GETUTCDATE()
     );
     ```
   - In `ApplicationOAuthProvider.cs`, implement `ValidateClientAuthentication` to query `[dbo].[api_clients]` via Dapper. Ensure the requesting `client_id` exists and has `is_active = 1` before issuing tokens.

4. **Full OAuth 2.0 Integration (OWIN Middleware + Tokens):**
   - **Endpoint `/token` (Password Grant):** Accepts `grant_type=password`, `username`, `password`, and `client_id`.
   - **Argon2id Integration:** Inside `GrantResourceOwnerCredentials`, look up the user in `[dbo].[user]` using Dapper and verify credentials using `UserService.VerifyPassword()`.
   - **Refresh Token Support:** Implement `AuthenticationTokenProvider` to store single-use refresh tokens in `[dbo].[refresh_tokens]`. All generated `id` fields must use `Guid.NewGuid().ToString()`.
   - **API Protection:** Replace custom `[TokenAuthorize]` attributes with standard OWIN `[Authorize]` attributes. Update `CurrentUserId` helpers to read `ClaimTypes.NameIdentifier` directly from `User.Identity`.

5. **Frontend Requirements (React Dashboard):**
   - **PRESERVE UI / DESIGN:** Do NOT redesign, replace, or alter the existing Login and Logout UI, styling, components, or layout.
   - **Update Under the Hood API Wiring Only:** 
     - Update `login.jsx` (or equivalent auth handler) to send `x-www-form-urlencoded` POST requests to `/token` passing `username`, `password`, `grant_type=password`, and `client_id=components_monitoring_react_app`.
     - Store the returned `access_token` and `refresh_token`.
     - Wire up the API client interceptor/helper to handle silent refresh when receiving 401 response codes.
     - Update the Logout action to call the token revocation endpoint.

6. **Exact Configuration Alignment:**
   - Restrict OWIN CORS policy strictly to `reactAppUrl` as configured in `Web.config`.
   - Apply the exact `Web.config` system.webServer handlers and modules from the sample project.
   - Match the `Startup.cs` pipeline order (CORS -> OAuth -> `app.UseWebApi(GlobalConfiguration.Configuration)`).

---

### Step-by-Step Execution Plan
1. Inspect the sample project files at the path provided above.
2. Review current `UserController.cs`, `UserService.cs`, and `Web.config` in `ComponentsMonitoringAPI`.
3. Generate SQL migration scripts for `[dbo].[refresh_tokens]` and `[dbo].[api_clients]`.
4. Add/update necessary OWIN NuGet packages if missing.
5. Generate/update `Startup.cs`, `ApplicationOAuthProvider.cs`, `ApplicationRefreshTokenProvider.cs`, `Web.config`, and `UserController.cs`.
6. Refactor React frontend auth methods under the hood while preserving 100% of the existing UI.