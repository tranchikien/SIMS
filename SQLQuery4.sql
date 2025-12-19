

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


GO
