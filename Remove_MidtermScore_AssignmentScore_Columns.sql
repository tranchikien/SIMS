-- Script to remove MidtermScore and AssignmentScore columns from Grades table
-- Execute this script in your SQL Server database

USE [YourDatabaseName]; -- Replace with your actual database name
GO

-- Check if columns exist before dropping them
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Grades]') AND name = 'MidtermScore')
BEGIN
    ALTER TABLE [dbo].[Grades]
    DROP COLUMN [MidtermScore];
    PRINT 'Column MidtermScore has been removed from Grades table.';
END
ELSE
BEGIN
    PRINT 'Column MidtermScore does not exist in Grades table.';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Grades]') AND name = 'AssignmentScore')
BEGIN
    ALTER TABLE [dbo].[Grades]
    DROP COLUMN [AssignmentScore];
    PRINT 'Column AssignmentScore has been removed from Grades table.';
END
ELSE
BEGIN
    PRINT 'Column AssignmentScore does not exist in Grades table.';
END
GO

PRINT 'Script execution completed.';
GO

