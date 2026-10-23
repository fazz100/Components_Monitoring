/*
   Application Group features migration (safe to run more than once).

   1. applications: alert_pending / failure_detected_at / failure_details
      (the health checker flags failures, ComponentsMonitoringAlertDispatcher emails them as group digests)
   2. application_exceptions: group-wide silencing via application_group_id
   3. response_code 903 (an exception must target an application OR an application group)

   Each ALTER that adds a column is in its own batch, because a batch that adds a column
   cannot reference that column (e.g. in a constraint) until the next batch.
*/

USE [component_monitoring]
GO

-- 1. Add alert pending and tracking columns to applications table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[applications]') AND name = 'alert_pending')
BEGIN
    ALTER TABLE [dbo].[applications] ADD [alert_pending] BIT NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[applications]') AND name = 'failure_detected_at')
BEGIN
    ALTER TABLE [dbo].[applications] ADD [failure_detected_at] DATETIME NULL;
END;
GO

-- reason for the latest failure, shown in the digest email
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[applications]') AND name = 'failure_details')
BEGIN
    ALTER TABLE [dbo].[applications] ADD [failure_details] NVARCHAR(MAX) NULL;
END;
GO

-- 2. Support Group-Level Exceptions in application_exceptions table
-- Allow application_id to be NULL for group-wide silencing
ALTER TABLE [dbo].[application_exceptions] ALTER COLUMN [application_id] NVARCHAR(50) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[application_exceptions]') AND name = 'application_group_id')
BEGIN
    ALTER TABLE [dbo].[application_exceptions] ADD [application_group_id] NVARCHAR(50) NULL;
END;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_application_exceptions_application_group')
BEGIN
    ALTER TABLE [dbo].[application_exceptions]
    ADD CONSTRAINT [FK_application_exceptions_application_group]
    FOREIGN KEY ([application_group_id]) REFERENCES [dbo].[application_group] ([id]);
END;
GO

-- 3. Response code for exceptions without a target
IF NOT EXISTS (SELECT * FROM [dbo].[response_code] WHERE [code] = 903)
BEGIN
    INSERT INTO [dbo].[response_code] ([code], [name], [message], [created_date])
    VALUES (903, 'ExceptionTargetRequired', 'Select either an application or an application group to silence.', getdate());
END;
GO
