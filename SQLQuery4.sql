-- =============================================
-- SQL Script: Insert/Update Admin User
-- =============================================
-- NOTE: Password will be automatically hashed
-- when the application runs for the first time.
-- The migration script will hash all plain text passwords.
-- =============================================

USE SIMSDB;
GO

-- Insert or update admin user
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin' AND Role = 'Admin')
BEGIN
    INSERT INTO Users (Username, Password, FullName, Email, Role, ReferenceId, Status)
    VALUES ('admin', '9999', 'System Administrator', 'admin@sims.edu', 'Admin', NULL, 'Active');
    
    PRINT 'Admin user created with plain text password.';
    PRINT 'Password will be automatically hashed when application starts.';
END
ELSE
BEGIN
    UPDATE Users 
    SET FullName = 'System Administrator',
        Email = 'admin@sims.edu',
        Status = 'Active'
    WHERE Username = 'admin' AND Role = 'Admin';
    
    PRINT 'Admin user updated.';
END
GO

-- IMPORTANT: After running this script, start the application.
-- The password migration will automatically hash the password.
-- Default password: 9999 (will be hashed on first run)
GO
