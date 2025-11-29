using SIMS.Models;

namespace SIMS.Repositories
{
    public interface IUserRepository
    {
        User? GetById(int id);
        User? GetByUsername(string username);
        User? GetByEmail(string email);
        User? GetByUsernameOrEmail(string usernameOrEmail);
        IEnumerable<User> GetAll();
        IEnumerable<User> GetByRole(string role);
        User? GetByReferenceId(int referenceId, string role);
        void Add(User user);
        void Update(User user);
        void Delete(int id);
        int GetCount();
    }
}

