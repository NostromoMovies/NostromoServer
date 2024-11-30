namespace Nostromo.Server.Database.Repositories
{
    public interface IImportFolderRepository : IRepository<ImportFolder>
    {
        Task<List<string>> GetWatchedPathsAsync();
        Task SaveWatchedPathsAsync(List<string> paths);
    }
}