# Components Monitoring API - Postman Test cURLs

**How to use these:**
- **Postman:** **Import → Raw text**, paste one cURL, then **Continue → Import**. Postman turns it into a request with the headers and body filled in.
- **Base URL:** the API runs as an IIS application at `/api`, and its routes also start with `api/`. So the token URL is `http://localhost/api/token`, while the endpoints are `http://localhost/api/api/...`. Replace `http://localhost` with your region's host.
- **Shell:** the commands use bash line continuations (`\`). For Windows cmd/PowerShell, import them into Postman, or join each command onto one line.
- **Responses:**
  - Every create endpoint returns the standard envelope `{ "Code": 200, "Message": "Success", "Data": ..., "Details": null }`.
  - On failure it returns **HTTP 500** with the exception in `Details`, or the business-error message from `response_code`.

---

## Step 1: Obtain a Bearer Token (Authentication)
Before calling the create endpoints, authenticate against the API to retrieve your access token.

```bash
curl -X POST "http://localhost/api/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=components_monitoring_react_app" \
  -d "username=YOUR_USERNAME" \
  -d "password=YOUR_PASSWORD"
```

The response contains `access_token` (valid for 15 minutes by default) and `refresh_token`. In every request below, replace `YOUR_ACCESS_TOKEN` with the `access_token` value.

| Response | Meaning |
|---|---|
| `400 {"error":"invalid_client"}` | `client_id` missing, or not an active row in `api_clients` |
| `400 {"error":"invalid_grant"}` | Wrong username or password, or the user is deleted |
| `400 {"error":"server_error"}` | Server problem (usually the pepper or the database). Check the API log |

**When the access token expires** (calls start returning 401), get a new one with the refresh token. Each refresh token works **once**:

```bash
curl -X POST "http://localhost/api/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=refresh_token" \
  -d "client_id=components_monitoring_react_app" \
  -d "refresh_token=YOUR_REFRESH_TOKEN"
```

---

## Step 2: Create an Application Group (System)
`POST api/application-groups/create`. `Data` in the response is the new group's **Id**; keep it for Steps 3 and 5.

```bash
curl -X POST "http://localhost/api/api/application-groups/create" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "Application_Group_Name": "Test System",
    "Description": "Created from Postman"
  }'
```

- `Application_Group_Name` is required (max 128 characters).
- A name that already exists returns code **902** (*Application group name already exists.*).

---

## Step 3: Create an Application (Component)
`POST api/applications/create`. `Data` is `true`.

**Windows service**

```bash
curl -X POST "http://localhost/api/api/applications/create" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "Application_Name": "DG3 Test Service",
    "Description": "Created from Postman",
    "Application_Type": "windows service",
    "IP_Address": "apppw01.us.dg3.local",
    "URL_Or_App_Name": "DG3TestService",
    "Working_Status": 1,
    "Service_Status": "Running",
    "Application_Group_Id": "GROUP_ID_FROM_STEP_2",
    "Databases": []
  }'
```

**Scheduled task**

```bash
curl -X POST "http://localhost/api/api/applications/create" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "Application_Name": "DG3 Test Task",
    "Description": "Created from Postman",
    "Application_Type": "scheduled task",
    "IP_Address": "apppw01.us.dg3.local",
    "URL_Or_App_Name": "DG3_Test_Task",
    "Working_Status": 1,
    "Is_Enabled": true,
    "Application_Group_Id": null,
    "Databases": []
  }'
```

**Website, with a database connection created in the same call**

```bash
curl -X POST "http://localhost/api/api/applications/create" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "Application_Name": "DG3 Test Website",
    "Description": "Created from Postman",
    "Application_Type": "website",
    "IP_Address": "wwwpw01.us.dg3.local",
    "URL_Or_App_Name": "https://test.dg3.com",
    "Working_Status": 1,
    "Application_Group_Id": "GROUP_ID_FROM_STEP_2",
    "Databases": [
      {
        "App_Database_Name": "TestDb",
        "Description": "Main database",
        "Connection_String": "Data Source=SQLSERVER;Initial Catalog=TestDb;Integrated Security=True;"
      }
    ]
  }'
```

**Field notes:**
- **`Databases` must always be sent**, even as `[]`. If it is left out, the API fails with a 500 (NullReferenceException in `ApplicationService.Create`).
- **`Application_Type`** must be exactly `"scheduled task"`, `"windows service"` or `"website"`.
- **`URL_Or_App_Name`** is the task name, service name or URL that the health checker uses.
- **`IP_Address`** is the server/host.
- **`Working_Status`:**

  | Value | Meaning |
  |---|---|
  | `0` | Not Working |
  | `1` | Working |
  | `2` | Known Issue |
  | `3` | No Longer Needed |

- **`Is_Enabled`** is used only for scheduled tasks, and **`Service_Status`** (`"Running"` / `"Stopped"`) only for Windows services. The API clears both for other types.
- **`Application_Group_Id`:** a group Id creates the pairing in `application_group_applications`. `null` or `""` makes the application standalone.
- **Server-generated fields:** `Id`, `Created_Date` and `Created_By` are set by the server; don't send them.
- **Finding the new application's Id:** the response is only `true`. Use `GET http://localhost/api/api/applications?appName=DG3%20Test` with the same `Authorization` header, then read `Id` from `Data`.

---

## Step 4: Add a Database Connection to an Existing Application
`POST api/application-databases/save`. `Data` is the new database row's **Id**.

```bash
curl -X POST "http://localhost/api/api/application-databases/save" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "App_Database_Name": "ReportingDb",
    "Description": "Reporting database",
    "Connection_String": "Data Source=SQLSERVER;Initial Catalog=ReportingDb;Integrated Security=True;",
    "Application_Id": "APPLICATION_ID"
  }'
```

- `App_Database_Name`, `Connection_String` and `Application_Id` are required.

---

## Step 5: Create an Exception (Silence Alerts)
`POST api/exceptions/create`. `Data` is `true`. Send **exactly one** of `Application_Id` or `Application_Group_Id`. Sending both or neither returns code **903** (*Select either an application or an application group to silence.*).

**Silence one application**

```bash
curl -X POST "http://localhost/api/api/exceptions/create" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "Application_Id": "APPLICATION_ID",
    "Application_Group_Id": null,
    "Reason_For_Exception": "Planned maintenance"
  }'
```

**Silence an entire system (application group)**

```bash
curl -X POST "http://localhost/api/api/exceptions/create" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "Application_Id": null,
    "Application_Group_Id": "GROUP_ID_FROM_STEP_2",
    "Reason_For_Exception": "System-wide maintenance"
  }'
```

- The silence lasts until `ComponentsMonitoringExceptionsUpdater` removes it after `days_silence_duration` days (default 7).

---

## Step 6: Create a User
`POST api/user/create`. `Data` is `true`.

```bash
curl -X POST "http://localhost/api/api/user/create" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "Username": "test.user",
    "PasswordString": "ChangeMe123!",
    "FirstName": "Test",
    "LastName": "User"
  }'
```

- `PasswordString` is the plain-text password; the server hashes it with Argon2id plus the pepper.
- An existing username returns code **901** (*UserExist*).
- To check the new account, call Step 1 with these credentials.

---

## Clean-up (optional)
The test rows can be removed from the dashboard:
- **Exceptions:** Exceptions page → trash icon.
- **Applications:** Applications page → delete (soft delete).
- **Application groups:** Application Groups page → delete (soft delete).

Or call the delete endpoints with the same `Authorization` header:
- `DELETE /api/api/exceptions/delete/{id}`
- `DELETE /api/api/applications/delete/{id}`
- `POST /api/api/application-groups/delete/{id}`
