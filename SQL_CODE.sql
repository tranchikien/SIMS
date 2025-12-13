
﻿-- 1. Tạo database
CREATE DATABASE SIMS_DB;
GO

USE SIMS_DB;
GO

-- 2. Tạo bảng Students
CREATE TABLE Students (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    StudentId   NVARCHAR(20) NOT NULL,    -- mã SV: SE001...
    FullName    NVARCHAR(100) NOT NULL,
    Email       NVARCHAR(100) NOT NULL,
    Program     NVARCHAR(100),
    Password    NVARCHAR(100),
    Role        NVARCHAR(20),
    Status      NVARCHAR(20)
);

-- 3. Tạo bảng Faculties
CREATE TABLE Faculties (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    FacultyId   NVARCHAR(20) NOT NULL,
    FullName    NVARCHAR(100) NOT NULL,
    Email       NVARCHAR(100) NOT NULL,
    Department  NVARCHAR(100),
    Password    NVARCHAR(100),
    Role        NVARCHAR(20),
    Status      NVARCHAR(20)
);

-- 4. Tạo bảng Courses
CREATE TABLE Courses (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    CourseCode  NVARCHAR(20) NOT NULL,
    CourseName  NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    Credits     INT NOT NULL,
    Status      NVARCHAR(20)
);

-- 5. Tạo bảng Enrollments
CREATE TABLE Enrollments (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    StudentId   INT NOT NULL,
    CourseId    INT NOT NULL,
    FacultyId   INT NOT NULL,
    Status      NVARCHAR(20),
    CONSTRAINT FK_Enrollment_Student FOREIGN KEY (StudentId) REFERENCES Students(Id),
    CONSTRAINT FK_Enrollment_Course  FOREIGN KEY (CourseId)  REFERENCES Courses(Id),
    CONSTRAINT FK_Enrollment_Faculty FOREIGN KEY (FacultyId) REFERENCES Faculties(Id)
);

-- 6. Tạo bảng Grades
CREATE TABLE Grades (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentId    INT NOT NULL,
    StudentId       INT NOT NULL,
    CourseId        INT NOT NULL,
    MidtermScore    DECIMAL(5,2) NULL,
    FinalScore      DECIMAL(5,2) NULL,
    AssignmentScore DECIMAL(5,2) NULL,
    TotalScore      DECIMAL(5,2) NULL,
    LetterGrade     NVARCHAR(5),
    Comment         NVARCHAR(500),
    CONSTRAINT FK_Grade_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id)
);



ALTER TABLE Grades
ADD FacultyId INT NULL;
ALTER TABLE Grades
ADD CONSTRAINT FK_Grades_Faculty 
FOREIGN KEY (FacultyId) REFERENCES Faculties(Id);



INSERT INTO Users (Username, Password, FullName, Email, Role, ReferenceId, Status)
VALUES ('admin', '9999', 'System Administrator', 'admin@sims.edu', 'Admin', NULL, 'Active');




ALTER TABLE Students
ADD Phone NVARCHAR(20),
    Address NVARCHAR(200),
    Gender NVARCHAR(10);

ALTER TABLE Faculties
ADD Phone NVARCHAR(20),
    Address NVARCHAR(200),
    Gender NVARCHAR(10);

ALTER TABLE AdminProfiles
ADD Phone NVARCHAR(20),
    Address NVARCHAR(200),
    Gender NVARCHAR(10);





USE SIMSDB;
GO


-- Create ActivityLogs Table

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLogs]') AND type in (N'U'))
BEGIN
    PRINT 'Creating ActivityLogs table...';
    
    CREATE TABLE ActivityLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ActivityType NVARCHAR(50) NOT NULL,
        GradeId INT NULL,
        StudentId INT NULL,
        CourseId INT NULL,
        FacultyId INT NULL,
        Description NVARCHAR(500) NOT NULL,
        OldValue NVARCHAR(2000) NULL,
        NewValue NVARCHAR(2000) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        PerformedBy NVARCHAR(100) NOT NULL
    );
    
    PRINT 'ActivityLogs table created successfully.';
END
ELSE
BEGIN
    PRINT 'ActivityLogs table already exists.';
    
    -- Check if columns exist and add missing ones
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLogs]') AND name = 'GradeId')
    BEGIN
        ALTER TABLE ActivityLogs ADD GradeId INT NULL;
        PRINT 'Added GradeId column.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLogs]') AND name = 'StudentId')
    BEGIN
        ALTER TABLE ActivityLogs ADD StudentId INT NULL;
        PRINT 'Added StudentId column.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLogs]') AND name = 'CourseId')
    BEGIN
        ALTER TABLE ActivityLogs ADD CourseId INT NULL;
        PRINT 'Added CourseId column.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLogs]') AND name = 'FacultyId')
    BEGIN
        ALTER TABLE ActivityLogs ADD FacultyId INT NULL;
        PRINT 'Added FacultyId column.';
    END
END
GO


-- Create Indexes for Better Performance

PRINT 'Creating indexes...';

-- Index for ActivityType
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_ActivityType' AND object_id = OBJECT_ID('ActivityLogs'))
BEGIN
    CREATE INDEX IX_ActivityLogs_ActivityType ON ActivityLogs(ActivityType);
    PRINT 'Created index IX_ActivityLogs_ActivityType.';
END

-- Index for StudentId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_StudentId' AND object_id = OBJECT_ID('ActivityLogs'))
BEGIN
    CREATE INDEX IX_ActivityLogs_StudentId ON ActivityLogs(StudentId);
    PRINT 'Created index IX_ActivityLogs_StudentId.';
END

-- Index for CourseId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_CourseId' AND object_id = OBJECT_ID('ActivityLogs'))
BEGIN
    CREATE INDEX IX_ActivityLogs_CourseId ON ActivityLogs(CourseId);
    PRINT 'Created index IX_ActivityLogs_CourseId.';
END

-- Index for FacultyId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_FacultyId' AND object_id = OBJECT_ID('ActivityLogs'))
BEGIN
    CREATE INDEX IX_ActivityLogs_FacultyId ON ActivityLogs(FacultyId);
    PRINT 'Created index IX_ActivityLogs_FacultyId.';
END

-- Index for CreatedAt (for sorting by date)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_CreatedAt' AND object_id = OBJECT_ID('ActivityLogs'))
BEGIN
    CREATE INDEX IX_ActivityLogs_CreatedAt ON ActivityLogs(CreatedAt);
    PRINT 'Created index IX_ActivityLogs_CreatedAt.';
END

GO


-- Verify Table Structure

PRINT '';
PRINT 'Verifying ActivityLogs table structure...';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ActivityLogs'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT 'Verifying indexes...';
SELECT 
    i.name AS IndexName,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('ActivityLogs')
ORDER BY i.name, ic.index_column_id;

PRINT '';
PRINT 'Setup completed successfully!';
GO




