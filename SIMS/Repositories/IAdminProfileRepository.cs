using SIMS.Models;

namespace SIMS.Repositories
{
    public interface IAdminProfileRepository
    {
        AdminProfile Get();
        void Save(AdminProfile profile);
    }
}

