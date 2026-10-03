:on error exit
USE [master];
IF DB_ID(N'tacdent') IS NULL
    CREATE DATABASE [tacdent];
GO
IF SUSER_ID(N'tacdent_app') IS NULL
    CREATE LOGIN [tacdent_app] WITH PASSWORD = N'$(APP_DB_PASSWORD)', CHECK_POLICY = ON, DEFAULT_DATABASE = [tacdent];
GO
USE [tacdent];
IF USER_ID(N'tacdent_app') IS NULL
    CREATE USER [tacdent_app] FOR LOGIN [tacdent_app] WITH DEFAULT_SCHEMA = [dbo];
ALTER ROLE [db_owner] ADD MEMBER [tacdent_app];
GO
SELECT name, collation_name, compatibility_level FROM sys.databases WHERE name = N'tacdent';
