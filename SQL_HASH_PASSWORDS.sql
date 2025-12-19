

USE SIMSDB;
GO

-- Check current password format (for verification)
-- Most passwords are currently stored as plain text
SELECT 
    Id,
    Username,
    Role,
    CASE 
        WHEN Password LIKE '$2%' THEN 'BCrypt Hash'
        WHEN LEN(Password) < 20 THEN 'Plain Text (likely)'
        ELSE 'Unknown Format'
    END AS PasswordFormat,
    LEN(Password) AS PasswordLength
FROM Users
ORDER BY Role, Username;
GO



PRINT '=============================================';
PRINT 'IMPORTANT: Run the C# migration script';
PRINT 'HashPasswordsMigration.cs to hash passwords.';
PRINT '=============================================';
GO

