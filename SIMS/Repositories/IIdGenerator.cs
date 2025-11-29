namespace SIMS.Repositories
{
    public interface IIdGenerator
    {
        int GetNextId(string entityType);
    }
}

