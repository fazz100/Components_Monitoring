/*
   OAuth 2.0 migration for an existing component_monitoring database.

   1. Creates [dbo].[refresh_tokens] (replaces [dbo].[user_session]).
   2. Creates [dbo].[api_clients] and registers the React dashboard client.
   3. (Optional, run manually) drops [dbo].[user_session] once the new API is deployed.
*/

USE [component_monitoring]
GO

CREATE TABLE [dbo].[refresh_tokens] (
    [id] NVARCHAR(50) NOT NULL PRIMARY KEY, -- String GUID
    [user_id] NVARCHAR(50) NOT NULL,        -- FK to [dbo].[user].[id]
    [token_hash] NVARCHAR(256) NOT NULL UNIQUE,
    [issued_at] DATETIME NOT NULL,
    [expires_at] DATETIME NOT NULL,
    [is_revoked] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_refresh_tokens_user] FOREIGN KEY ([user_id]) REFERENCES [dbo].[user] ([id])
);
GO

CREATE TABLE [dbo].[api_clients] (
    [id] NVARCHAR(50) NOT NULL PRIMARY KEY, -- String GUID
    [client_name] NVARCHAR(100) NOT NULL,    -- e.g., 'Components Monitoring Dashboard'
    [client_id] NVARCHAR(100) NOT NULL UNIQUE,
    [client_secret_hash] NVARCHAR(256) NULL,
    [is_active] BIT NOT NULL DEFAULT 1,
    [created_date] DATETIME NOT NULL DEFAULT GETUTCDATE()
);
GO

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
GO


/* ---------------------------------------------------------------------------
   3. Run ONLY after the OAuth API is deployed and verified.
      [user_session] is no longer used by any code.
--------------------------------------------------------------------------- */
-- DROP TABLE [dbo].[user_session];
-- GO
