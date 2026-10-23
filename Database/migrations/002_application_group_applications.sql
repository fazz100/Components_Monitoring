/*
   Application group membership moves into [application_group_applications] (safe to run more than once).
   Run after 001_application_group_features.sql.

   1. application_group becomes a pure lookup table: drop the unused application_id column.
   2. Create application_group_applications (one system per application).
   3. Copy existing applications.application_group_id assignments into it, then drop that column.

   Statements that reference a column which may already be dropped run through sp_executesql,
   because a static batch referencing a missing column fails to compile on a second run.
*/

USE [component_monitoring]
GO

-- 1. application_group.application_id (unused leftover column)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[application_group]') AND name = 'application_id')
BEGIN
    DECLARE @dropFks NVARCHAR(MAX) = N'';

    SELECT @dropFks = @dropFks + N'ALTER TABLE [dbo].[application_group] DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';'
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
    JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
    WHERE fkc.parent_object_id = OBJECT_ID(N'[dbo].[application_group]') AND c.name = 'application_id';

    IF @dropFks <> N'' EXEC sp_executesql @dropFks;

    EXEC sp_executesql N'ALTER TABLE [dbo].[application_group] DROP COLUMN [application_id];';
END;
GO

-- 2. membership table
IF OBJECT_ID(N'[dbo].[application_group_applications]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[application_group_applications]
    (
    id nvarchar(50) not null PRIMARY KEY
    ,application_group_id nvarchar(50) not null
    ,application_id nvarchar(50) not null
    ,CONSTRAINT [UQ_application_group_applications_application] UNIQUE ([application_id]) -- one system per application
    ,CONSTRAINT [FK_application_group_applications_application_group] FOREIGN KEY ([application_group_id]) REFERENCES [dbo].[application_group] ([id])
    ,CONSTRAINT [FK_application_group_applications_applications] FOREIGN KEY ([application_id]) REFERENCES [dbo].[applications] ([id])
    );
END;
GO

-- 3. move applications.application_group_id into the membership table, then drop the column
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[applications]') AND name = 'application_group_id')
BEGIN
    EXEC sp_executesql N'
        INSERT INTO [dbo].[application_group_applications] ([id], [application_group_id], [application_id])
        SELECT LOWER(NEWID()), a.[application_group_id], a.[id]
        FROM [dbo].[applications] a
        WHERE a.[application_group_id] IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM [dbo].[application_group_applications] aga WHERE aga.[application_id] = a.[id]);';

    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_applications_application_group')
        ALTER TABLE [dbo].[applications] DROP CONSTRAINT [FK_applications_application_group];

    EXEC sp_executesql N'ALTER TABLE [dbo].[applications] DROP COLUMN [application_group_id];';
END;
GO
