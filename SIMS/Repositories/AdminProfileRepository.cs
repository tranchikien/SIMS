using SIMS.Data;
using SIMS.Models;

namespace SIMS.Repositories
{
    public class AdminProfileRepository : IAdminProfileRepository
    {
        private readonly SIMSDbContext _context;

        public AdminProfileRepository(SIMSDbContext context)
        {
            _context = context;
        }

        public AdminProfile Get()
        {
            // Try to get from AdminProfiles table first (for backward compatibility)
            var profile = _context.AdminProfiles.FirstOrDefault();
            
            // Get from Users table (Role = Admin)
            var adminUser = _context.Users.FirstOrDefault(u => u.Role == "Admin");
            
            if (adminUser != null)
            {
                // Map from User to AdminProfile
                return new AdminProfile
                {
                    Username = adminUser.Username,
                    FullName = adminUser.FullName,
                    Email = adminUser.Email,
                    Role = "System Administrator",
                    Phone = adminUser.Phone,
                    Address = adminUser.Address,
                    Gender = adminUser.Gender
                };
            }
            
            if (profile == null)
            {
                // Create default admin profile if none exists
                profile = new AdminProfile
                {
                    Username = "admin",
                    FullName = "Dr. Nguyen Van X",
                    Email = "admin@sims.edu",
                    Role = "System Administrator"
                };
                _context.AdminProfiles.Add(profile);
                _context.SaveChanges();
            }
            return profile;
        }

        public void Save(AdminProfile profile)
        {
            // Save to AdminProfiles table (for backward compatibility)
            var existing = _context.AdminProfiles.Find(profile.Username);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(profile);
            }
            else
            {
                _context.AdminProfiles.Add(profile);
            }
            
            // Save Phone, Address, and Gender to Users table
            var adminUser = _context.Users.FirstOrDefault(u => u.Role == "Admin" && u.Username == profile.Username);
            if (adminUser != null)
            {
                // Update Phone, Address, and Gender (editable fields)
                adminUser.Phone = profile.Phone;
                adminUser.Address = profile.Address;
                adminUser.Gender = profile.Gender; // Now editable
                _context.Users.Update(adminUser);
            }
            
            _context.SaveChanges();
        }
    }
}

