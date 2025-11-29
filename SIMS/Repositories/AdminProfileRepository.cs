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
            var profile = _context.AdminProfiles.FirstOrDefault();
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
            var existing = _context.AdminProfiles.Find(profile.Username);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(profile);
            }
            else
            {
                _context.AdminProfiles.Add(profile);
            }
            _context.SaveChanges();
        }
    }
}

