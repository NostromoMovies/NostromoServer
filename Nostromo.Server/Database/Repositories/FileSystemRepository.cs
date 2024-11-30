using System.Collections.Generic;
using System.Linq;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;

namespace Nostromo.Server.Database.Repositories;

// Modify IFileSystemRepository interface
public interface IFileSystemRepository
{
    Task<List<string>> LoadWatchedPathsAsync();  // Asynchronous method
    Task SaveWatchedPathsAsync(List<string> paths);  // Asynchronous method for saving paths
}


public class FileSystemRepository : IFileSystemRepository
{
    private readonly NostromoDbContext _dbContext;

    public FileSystemRepository(NostromoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Asynchronous method for loading watched paths
    public async Task<List<string>> LoadWatchedPathsAsync()
    {
        // Asynchronously query the ImportFolder table to get all watched paths (where IsWatched == 1)
        return await _dbContext.ImportFolders
                               .Where(f => f.IsWatched == 1)
                               .Select(f => f.FolderLocation)
                               .ToListAsync();  // Use ToListAsync for asynchronous query execution
    }

    // Asynchronous method for saving watched paths
    public async Task SaveWatchedPathsAsync(List<string> paths)
    {
        var importFolders = await _dbContext.ImportFolders
                                            .Where(f => paths.Contains(f.FolderLocation))
                                            .ToListAsync();

        foreach (var folder in importFolders)
        {
            folder.IsWatched = 1;
        }

        var newPaths = paths.Except(importFolders.Select(f => f.FolderLocation)).ToList();
        var newImportFolders = newPaths.Select(path => new ImportFolder
        {
            FolderLocation = path,
            IsWatched = 1,
            IsDropSource = 0,
            IsDropDestination = 0,
            ImportFolderType = 0
        }).ToList();

        await _dbContext.ImportFolders.AddRangeAsync(newImportFolders);  // Use AddRangeAsync for async insertion
        await _dbContext.SaveChangesAsync();
    }
}
