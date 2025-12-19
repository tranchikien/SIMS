// =============================================
// C# Migration Script: Hash Existing Passwords
// =============================================
// This script hashes all existing plain text passwords in the database
// using BCrypt.
//
// Instructions:
// 1. Add BCrypt.Net-Next NuGet package to your project (if not already added)
// 2. Update the connection string below
// 3. Run this script in a console application or add it to your Program.cs
// =============================================

using Microsoft.EntityFrameworkCore;
using SIMS.Data;
using SIMS.Models;
using BCrypt.Net;

namespace SIMS.Migrations
{
    public class HashPasswordsMigration
    {
        public static void Run(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SIMSDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new SIMSDbContext(optionsBuilder.Options);
            
            Console.WriteLine("Starting password hashing migration...");
            
            var users = context.Users.ToList();
            int hashedCount = 0;
            int skippedCount = 0;
            
            foreach (var user in users)
            {
                // Check if password is already hashed (BCrypt hashes start with $2)
                if (user.Password.StartsWith("$2"))
                {
                    Console.WriteLine($"Skipping {user.Username} - password already hashed");
                    skippedCount++;
                    continue;
                }
                
                // Hash the plain text password
                try
                {
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
            
            Console.WriteLine($"\nMigration completed!");
            Console.WriteLine($"Hashed: {hashedCount} passwords");
            Console.WriteLine($"Skipped: {skippedCount} passwords (already hashed)");
        }
    }
}

// =============================================
// To run this migration, add this to Program.cs:
// =============================================
// using SIMS.Migrations;
// 
// // In Main method, after database connection is established:
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// HashPasswordsMigration.Run(connectionString);
// =============================================

