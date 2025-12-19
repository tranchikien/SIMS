// =============================================
// C# Migration Script: Hash Existing Passwords
// =============================================
// This script hashes all existing plain text passwords in the database
// using BCrypt.
//
// Instructions:
// 1. This will be called automatically on application startup
// 2. It only runs once (checks if passwords are already hashed)
// =============================================

using Microsoft.EntityFrameworkCore;
using SIMS.Data;
using SIMS.Models;
using BCrypt.Net;

namespace SIMS.Migrations
{
    public class HashPasswordsMigration
    {
        public static void Run(SIMSDbContext context)
        {
            try
            {
                Console.WriteLine("Checking if password migration is needed...");
                
                var users = context.Users.ToList();
                if (users.Count == 0)
                {
                    Console.WriteLine("No users found. Skipping password migration.");
                    return;
                }
                
                // Check if any passwords need hashing
                var usersNeedingHash = users.Where(u => !u.Password.StartsWith("$2")).ToList();
                
                if (usersNeedingHash.Count == 0)
                {
                    Console.WriteLine("All passwords are already hashed. Skipping migration.");
                    return;
                }
                
                Console.WriteLine($"Found {usersNeedingHash.Count} users with plain text passwords. Starting migration...");
                
                int hashedCount = 0;
                
                foreach (var user in usersNeedingHash)
                {
                    try
                    {
                        // Hash the plain text password
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password, workFactor: 12);
                        user.Password = hashedPassword;
                        context.Users.Update(user);
                        
                        Console.WriteLine($"Hashed password for {user.Username} ({user.Role})");
                        hashedCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error hashing password for {user.Username}: {ex.Message}");
                    }
                }
                
                context.SaveChanges();
                
                Console.WriteLine($"Password migration completed! Hashed {hashedCount} passwords.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during password migration: {ex.Message}");
                // Don't throw - allow application to continue
            }
        }
    }
}

