create database component_monitoring;

USE component_monitoring;
GO

create table [applications]
(
id nvarchar(50) not null PRIMARY KEY  -- guid
,application_name nvarchar(256) not null 
,[description] nvarchar(256) null 
,application_type nvarchar(50) null --scheduled task, service, website
,ip_address nvarchar(256) -- ip address or server which the app belongs
,url_or_app_name nvarchar(256) null --for checking which app name to check / url of landing page to ping
,[working_status] int not null default 1--0 not working, 1 working, 2 known issue, 3 no longer needed
,is_enabled bit null --for scheduled task, enabled or not, nullable in case of sites and win services
,service_status nvarchar(100) null --if running, stopped for win services
,max_allowed_age int null
,created_date datetime not null
,created_by nvarchar(50) not null
,updated_date datetime null
,updated_by nvarchar(50) null
,is_deleted bit default 0 not null
)

CREATE TABLE [dbo].[user](
	[id] [nvarchar](50) NOT NULL,
	[username] [nvarchar](50) NULL,
	[password] [varbinary](max) NULL,
	[first_name] [nvarchar](100) NULL,
	[last_name] [nvarchar](100) NULL,
	[created_date] [datetime] NULL,
	[created_by] [bigint] NULL,
	[updated_date] [datetime] NULL,
	[updated_by] [bigint] NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO



/****** Object:  Table [dbo].[user_session]    Script Date: 4/3/2026 4:10:48 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[user_session](
	[id] [nvarchar](50) NOT NULL,
	[user_id] [nvarchar](50) NOT NULL,
	[token] [nvarchar](256) NOT NULL,
	[issued_at] [datetime] NOT NULL,
	[expires_at] [datetime] NOT NULL,
	[is_revoked] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[token] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[token] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[user_session] ADD  DEFAULT (getutcdate()) FOR [issued_at]
GO

ALTER TABLE [dbo].[user_session] ADD  DEFAULT ((0)) FOR [is_revoked]
GO

ALTER TABLE [dbo].[user_session]  WITH CHECK ADD  CONSTRAINT [FK_user_session_user] FOREIGN KEY([user_id])
REFERENCES [dbo].[user] ([id])
GO

ALTER TABLE [dbo].[user_session] CHECK CONSTRAINT [FK_user_session_user]
GO



USE [component_monitoring]
GO

/****** Object:  Table [dbo].[response_code]    Script Date: 4/14/2026 1:34:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[response_code](
	[code] [int] NULL,
	[name] [nvarchar](255) NULL,
	[message] [nvarchar](500) NULL,
	[created_date] [datetime] NULL
) ON [PRIMARY]
GO


INSERT INTO [dbo].[response_code]
           ([code]
           ,[name]
           ,[message]
           ,[created_date])
     VALUES
           (200
           ,'Success'
           ,'Success'
           ,getdate())
GO

INSERT INTO [dbo].[response_code]
           ([code]
           ,[name]
           ,[message]
           ,[created_date])
     VALUES
           (999
           ,'Internal Server Error'
           ,'Internal Server Error'
           ,getdate())
GO


INSERT INTO [dbo].[response_code]
           ([code]
           ,[name]
           ,[message]
           ,[created_date])
     VALUES
           (901
           ,'UserExist'
           ,'UserExist'
           ,getdate())
GO

create table application_exceptions
(
id nvarchar(50) not null PRIMARY KEY
,application_id nvarchar(50) not null
,reason_for_exception nvarchar(300) null
,created_by nvarchar(50) null
,created_date datetime
)


create table [application_databases]
(
id nvarchar(50) not null PRIMARY KEY
,[app_database_name] nvarchar(128) not null
,[description] nvarchar(256) null
,[connection_string] nvarchar(max) not null
,application_id nvarchar(50) not null
,created_by nvarchar(50) null
,created_date datetime
)


GO





declare @admin nvarchar(50) = (select id from [user] where [username] = 'admin')  --admin id


--scheduled tasks
INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard Automated Sensitive Data Report'
,'Dashboard Automated Sensitive Data Report'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard Automated Sensitive Data Report'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard Distrobution Updater'
,'Scheduled task to update Dashboard job distributions once they''re shipped in Monarch'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard Distrobution Updater'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard Job Updater'
,'Syncs Dashboard Job details with that of Monarch'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard Job Updater'
,1
,getdate()
,@admin
,0)



INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard Job Updater (Check Special WoAction)'
,'Dashboard Job Updater (Check Special WoAction)'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard Job Updater (Check Special WoAction)'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard Job Updater (CSR Mapping)'
,'Dashboard Job Updater (CSR Mapping)'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard Job Updater (CSR Mapping)'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard Job Updater (Failure To Pickup Reminder)'
,'Sends email reminder for sent work orders that are not picked up by the vendors'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard Job Updater (Failure To Pickup Reminder)'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard Job Updater (Non NA WO Closer)'
,'Dashboard Job Updater (Non NA WO Closer)'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard Job Updater (Non NA WO Closer)'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard Rolodex Auto Deactivator Ongoing'
,'Rolodex auto deactivator task'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard Rolodex Auto Deactivator Ongoing'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard Rolodex List Updater'
,'Syncronizes Dashboard''s distributions lists with Monarch''s shipping lists details. (DashboardDistributionUpdater -js)'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard Rolodex List Updater'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard SharePoint AutoFolder'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard SharePoint AutoFolder'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard Sharepoint Library Count Notifier'
,'Task that counts the active Jobs libraries in SharePoint'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard Sharepoint Library Count Notifier'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard VDP OnDemand Report'
,'Task that sends notification about VDP Work Orders that have mail dates set as the current date and On Demand field set to Yes'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard VDP OnDemand Report'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard Vendor File Archiver'
,'Queries Monarch for recently shiiped jobs and sends it to Vendor web services for file archiving'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard Vendor File Archiver'
,1
,getdate()
,@admin
,0)


INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard WO Completer'
,'Auto completes Dashboard WOs that are past their due date'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dashboard WO Completer'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dg3Workshare Check Library Count'
,'Dg3Workshare Check Library Count'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Dg3Workshare Check Library Count'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'MS Auto Delete Rule'
,'MS Auto Delete Rule'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'MS Auto Delete Rule'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'OBP Under par level email notification'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'OBP Under par level email notification'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Send Machine Status Email'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Send Machine Status Email'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Update OBP Inventory'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'Update OBP Inventory'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Main job creator'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\AIPAC\Main job creator'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Monarch Order Exporter'
,'Creates job tickets for the AIPAC Shopify orders that have been exported to Propago'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\AIPAC\Monarch Order Exporter'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Shopify Updater'
,'Updates Shopify with shipping info once an order has shipped'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\AIPAC\Shopify Updater'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Catalogue Refresh'
,'Retrieves BNY catalogue from Propago, generates the nightly refresh file and uploads it to client FTP site'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\BNY\Catalogue Refresh'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'BNY New Version'
,'Handle new product versions in the BNY LIVE system'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\BNY SUBS\LIVE\BNY New Version'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'BNY Refresh Products'
,'Refresh UAT BNY Subscription product list from Propago'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\BNY SUBS\LIVE\BNY Refresh Products'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Brava Monarch Export'
,'Export pending jobs to Monarch'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\BRAVA\Brava Monarch Export'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Export Brava orders to Propago'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\BRAVA\Export Brava orders to Propago'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Get shipments'
,'Retrieve shipping updates from Propago'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\BRAVA\Get shipments'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Parse Brava orders'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\BRAVA\Parse Brava orders'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Shopify Updater'
,'Send shipping updates to Shopify'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\BRAVA\Shopify Updater'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'[NEW] PROD_Dg3.DiggyAI.WeeklyReport'
,'Send weekly Diggy report'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\DG3_WEBSITE\[NEW] PROD_Dg3.DiggyAI.WeeklyReport '
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'FMI Import Process'
,'Imports new jobs from Facilis into Monarch'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\FACILIS\FMI Import Process'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'FMI Update Process'
,'Updates Facilis jobs imported into Monarch with latest data from Facilis'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\FACILIS\FMI Update Process'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'GAF BS-Propago Part Sync Report'
,'Sends a report detailing skus that have been changed in Propago and also exist on the BS site'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\GAF\GAF BS-Propago Part Sync Report'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'GAF Inventory Report'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\GAF\GAF Inventory Report'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'GAF Job Status Report - Internal'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\GAF\GAF Job Status Report - Internal'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PROD - Generate PDF Proof'
,'Generate PDF Proofs for Pending Orders'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\GS_STATIONERY\PROD - Generate PDF Proof'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PROD - Order Import'
,'Imports new orders from Promail into App Database'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\GS_STATIONERY\PROD - Order Import'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PROD - Send cXML to Ariba'
,'Posts pending cxmls in the system to Ariba'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\GS_STATIONERY\PROD - Send cXML to Ariba'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PROD - Send Proof Approval Email Reminder'
,'Send reminder emails for pending proofs. Reminder to be sent every three days after initial email. Up to two reminder emails.'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\GS_STATIONERY\PROD - Send Proof Approval Email Reminder'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PROD - Send To Vendor'
,'Sends order to vendor'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\GS_STATIONERY\PROD - Send To Vendor'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PROD - Sync Promail UOMs'
,'syncs Promail''s unit of measures into app database'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\GS_STATIONERY\PROD - Sync Promail UOMs'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PROD - Vendor Tracking Report'
,'Sends a report showing orders for which tracking info was received in the last 24 hours and timestamp of last shipping notice received'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\GS_STATIONERY\PROD - Vendor Tracking Report'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard non-VDP Process'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\ICARUS\Dashboard non-VDP Process'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard VDP Process'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\ICARUS\Dashboard VDP Process'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PSEG DigiPrint'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\ICARUS\PSEG DigiPrint'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Bulk Order Exporter'
,'Exports pending Lumeris cXML bulk orders received from RBO through the web service into Promail'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\LUMERIS\Bulk Order Exporter'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Order Status Updater'
,'Retrieves status updates from the Promail database to mark items as shipped'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\LUMERIS\Order Status Updater'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Ship Notice Sender'
,'Sends tracking report to RBO FTPS site for any shipped Lumeris orders'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\LUMERIS\Ship Notice Sender'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 Monarch CSV Export'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\MONARCH\DG3 Monarch CSV Export'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'LIVE MS Recon Complete'
,'Processes completed batch files, emails doc acknowledgement files and archives files'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\MSRECON\LIVE MS Recon Complete'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'LIVE MS Recon Monarch Auto Complete'
,'Auto Complete the files in the system that was 89 or 73''d in Monarch'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\MSRECON\LIVE MS Recon Monarch Auto Complete'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'LIVE MS Recon Monarch Inventory Report'
,'Generates Monarch Inventory Report'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\MSRECON\LIVE MS Recon Monarch Inventory Report'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'LIVE MS Recon Process'
,'Processes PGP files from FTP server and emails trans acknowledgement files'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\MSRECON\LIVE MS Recon Process'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PROD - NBI File Process'
,'Process JSON files in FTP'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\NBI\PROD - NBI File Process'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 OBP Clicks Data Sharepoint Export'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\DG3 OBP Clicks Data Sharepoint Export'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dg3 OBP Clicks Exporter'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\Dg3 OBP Clicks Exporter'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 OBP Machine History Sharepoint Export'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\DG3 OBP Machine History Sharepoint Export'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 OBP Machine Status Event Sharepoint Export'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\DG3 OBP Machine Status Event Sharepoint Export'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 OBP Parts and Supply Sharepoint Export'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\DG3 OBP Parts and Supply Sharepoint Export'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dg3 OBP Parts Click Count Sharepoint Export'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\Dg3 OBP Parts Click Count Sharepoint Export'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 OBP Ships Margin Report Sharepoint Export'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\DG3 OBP Ships Margin Report Sharepoint Export'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dg3.OBP.CsvFileExporter'
,'This app updates the csv source file of OBP Inventory Reporting'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\Dg3.OBP.CsvFileExporter'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dg3.OBP.OrderForecast'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\Dg3.OBP.OrderForecast'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'OBP Daily Maintenance Report'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\OBP Daily Maintenance Report'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'OBP Daily Maintenance Report - Not Performed'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\OBP Daily Maintenance Report - Not Performed'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'OBP Machine Error Code Parser'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\OBP Machine Error Code Parser'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'OBP Open Order PO Reminder'
,'Send email reminder to user 16 days before the warehouse date.'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\OBP Open Order PO Reminder'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'OBP Parts Click Count Report'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\OBP\OBP Parts Click Count Report'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PiPlusDataSync'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\PI PLUS\PiPlusDataSync'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PiPlusShipdateUpdater'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\PI PLUS\PiPlusShipdateUpdater'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PSEG LIVE Approval Reminder'
,'Sends out reminder emails if any orders need approving'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\PSEG\PSEG LIVE Approval Reminder'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PSEG LIVE Status Updater'
,'Updates the PSEG status of shipped orders from Monarch'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\PSEG\PSEG LIVE Status Updater'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PSEG Print Email Confirmation'
,'PSEG Print Email Confirmation'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\PSEG\PSEG Print Email Confirmation'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DpsiDox Status Maintenance'
,'Deletes old status update entries to avoid system issues with overfull lists'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\SP_MAINTENANCE\DpsiDox Status Maintenance'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS AQR Data Feed Processor'
,'Downloads feeds from ftp folder, generates CSV data files and PNG files and reuploads to site data folder'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\UBS\UBS AQR Data Feed Processor'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS Cantor Data Feed Processor'
,''
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\UBS\UBS Cantor Data Feed Processor'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS Global Data Feed Monitor'
,'Sends Email Alerts for missed data feed schedule'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\UBS\UBS Global Data Feed Monitor'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS Global Liquidity Data Feed Processor'
,'Downloads feeds from ftp folder, generates CSV and JSON data files and reuploads to site data folder'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\UBS\UBS Global Liquidity Data Feed Processor'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS Global Liquidity Funds Report'
,'Sends a funds list report'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\UBS\UBS Global Liquidity Funds Report'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS Mailings Portal Monthly Job Creator'
,'Creates monthly parent job for UBS Mailings Job'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\UBS\UBS Mailings Portal Monthly Job Creator'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS MMF Data Feed Processor'
,'Downloads feeds from ftp folder, generates CSV data files and PNG files and reuploads to site data folder'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\UBS\UBS MMF Data Feed Processor'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS Short Duration Data Feed Processor'
,'Downloads feeds from ftp folder, generates CSV data files and PNG files and reuploads to site data folder'
,'scheduled task'
,'DG3365-APPPW01.dg3client.local'
,'\UBS\UBS Short Duration Data Feed Processor'
,1
,getdate()
,@admin
,0)



--Windows Service
INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 Aipac Core Order Service'
,''
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'Dg3.Aipac.CoreOrderService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 BNY Monarch Integration Service'
,'Service that processes incoming order files from Propago for BNY'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'Dg3.Bny.MonarchIntegrationService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 Dashboard Action Email Notification Service'
,'Adding action emailer'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'Dg3.Dashboard.ActionEmailNotificationService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 Dashboard Distro Requeue Service'
,'Requeues messages stranded in the Hagen Distro Exporter service''s working queue back to the request queue'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'Dg3.Dashboard.DistroRequeueService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 Dashboard Email Service'
,'Emails distribution files to Dashboard Clients'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'dg3_DashboardEmailSvc'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 Dashboard Hagen Service'
,'Exports Dashboard''s distribution items to Hagen''s shipping'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'dg3_DashboardHagenSvc'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dg3 Dashboard Monarch Auto Po Service'
,'Monarch Auto PO, finds the corresponding Dashboard Cost Batch Id for External Vendors and upload to Monarch'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'Dg3.Dashboard.MonarchAutoPoService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 Dashboard SharePoint AutoFolder Service'
,'Access Queueing messages that creates document libraries in Sharepoint'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'Dg3.Dashboard.SharePointAutoFolderService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 Dashboard Vendor Hooks Service'
,'Accepts Vendor Hook Messages that posts data to Client Webservice URL'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'Dg3.Dashboard.VendorHooksService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 Hagen Access Service'
,'Service that hosts the HagenDataAccess object, which provides data access to Hagen to .NET remoting clients'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'Dg3.HagenAccessService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 Live BNY Catalog Refresh Service Api Caller'
,''
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'DG3 Live BNY Catalog Refresh Service Api Caller'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 Live BNY Windows Service Api Caller'
,''
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'DG3 Live BNY Windows Service Api Caller'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3 UBS Mailing Portal Service'
,'UBS Daily Mailings Production Automation of Job Creation and File Moving'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'Dg3.UBS.MailingsPortal.Service'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dg3.Pseg.InvoiceGeneratorService'
,'Generates invoices for PSEG orders that are reviewed by billing'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'Dg3.Pseg.InvoiceGeneratorService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3_FundINSiteCacheRefresherService'
,'Calls the Umbraco Cache Refresh web service'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'DG3_FundINSiteCacheRefresherService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3'
,''
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'DG3'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3_FundINSiteIP2LocationService'
,'Service that hosts the IP2Location component, which provides geographical data from IP addresses to .NET remoting clients'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'DG3_FundINSiteIP2LocationService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3_FundINSitePackingSlipService'
,'Generates the shipping slip PDF for FundINSite'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'DG3_FundINSitePackingSlipService'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3_FundINSiteXbrlProcessorSvc'
,'FundINSite service to call the XBRL ReportBuilderRender process to generate HTML renderings of XBRL documents'
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'DG3_FundINSiteXbrlProcessorSvc'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'DG3_i-versionInsiteGeneratorInstance1'
,''
,'windows service'
,'DG3365-APPPW01.dg3client.local'
,'DG3_i-versionInsiteGeneratorInstance1'
,1
,getdate()
,@admin
,0)


---Websites
INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'BNY Mellon Dreyfus E-Delivery'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'bnymellon.dg3.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'BNY Subscriptions'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'bny-subs.dg3.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Dashboard'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'dashboard.dg3.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'FundINSite'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'www.fundinsite.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'FundINSite Admin'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'www.admin.fundinsite.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'GAF POD SSO'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'gafmerch.dg3.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'GS Stationery Admin Site'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'gs-admin-site.dg3.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'IT Portal'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'itportal.dg3.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'i-version insite Admin site'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'www.admin.insite.i-version.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Onboard Print'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'onboardprint.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PI Plus'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'clients.piplus.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PSEG'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'pseg.dg3.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PSEG Print'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'psegprint.dg3.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PSEG Promo Form'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'pseg-form.dg3.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'PSEG Return Form'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'pseg-return.dg3.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS AQR Fund Microsite'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'www.lpcif.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS Cantor'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'cantormmf.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS Global'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'www.globalliquiditymanagement.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS Liquidity'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'www.usliquidity.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS Mailings Portal'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'ubsmailingportal.dg3.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS Short Duration'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'www.usshortduration.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'UBS US Money Markets'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'www.usmoneymarketfunds.com'
,1
,getdate()
,@admin
,0)

INSERT INTO [dbo].[applications]
([id]
,[application_name]
,[description]
,[application_type]
,[ip_address]
,[url_or_app_name]
,[is_enabled]
,[created_date]
,[created_by]
,[max_allowed_age])
VALUES
(NEWID()
,'Vsion'
,''
,'website'
,'DG3365-WWWPW01.dg3client.local'
,'vsion.dg3.com'
,1
,getdate()
,@admin
,0)

GO
