namespace SIMS.Repositories
{
    public class IdGenerator : IIdGenerator
    {
        // Entity Framework Core uses identity columns, so IDs are auto-generated
        // This method is kept for backward compatibility but returns 0
        // EF Core will ignore 0 and auto-generate the ID
        public int GetNextId(string entityType)
        {
            return 0; // EF Core will auto-generate the ID
        }
    }
}

