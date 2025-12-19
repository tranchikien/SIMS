-- =============================================
-- SQL Script: Hash Existing Passwords
-- =============================================
-- NOTE: SQL Server does not have built-in BCrypt hashing.
-- This script provides instructions for hashing passwords.
-- 
-- You need to run a C# migration script to hash existing passwords.
-- See: HashPasswordsMigration.cs
-- =============================================

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

-- =============================================
-- IMPORTANT: After running the C# migration script,
-- all passwords will be hashed using BCrypt.
-- =============================================

-- Verify passwords are hashed (after migration)
-- SELECT 
--     Id,
--     Username,
--     Role,
--     CASE 
--         WHEN Password LIKE '$2%' THEN 'BCrypt Hash'
--         ELSE 'NOT HASHED - NEEDS MIGRATION'
--     END AS PasswordFormat
-- FROM Users
-- WHERE Password NOT LIKE '$2%';
-- GO

PRINT '=============================================';
PRINT 'IMPORTANT: Run the C# migration script';
PRINT 'HashPasswordsMigration.cs to hash passwords.';
PRINT '=============================================';
GO

