-- 1. Tạo database
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