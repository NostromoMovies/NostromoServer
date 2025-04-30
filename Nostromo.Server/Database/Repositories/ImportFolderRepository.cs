using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;

public class ImportFolderRepository : Repository<ImportFolder>, IImportFolderRepository
{
    public ImportFolderRepository(NostromoDbContext context) : base(context)
    {
    }

    public async Task<List<string>> GetWatchedPathsAsync()
    {
        return await Query()
            .Where(f => f.IsWatched == 1)
            .Select(f => f.FolderLocation)
            .ToListAsync();
    }

    public async Task SaveWatchedPathsAsync(List<string> paths)
    {
        // Get existing folders that match the provided paths
        var existingFolders = await Query()
            .Where(f => paths.Contains(f.FolderLocation))
            .ToListAsync();

        // Update IsWatched for existing folders
        foreach (var folder in existingFolders)
        {
            folder.IsWatched = 1;
            await UpdateAsync(folder);
        }

        // Find paths that don't exist in the database
        var existingPaths = existingFolders.Select(f => f.FolderLocation);
        var newPaths = paths.Except(existingPaths);

        // Create and add new folders
        foreach (var path in newPaths)
        {
            var newFolder = new ImportFolder
            {
                FolderLocation = path,
                IsWatched = 1,
                IsDropSource = 0,
                IsDropDestination = 0,
                ImportFolderType = 0
            };
            await AddAsync(newFolder);
        }
    }
}