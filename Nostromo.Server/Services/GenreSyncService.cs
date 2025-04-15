using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nostromo.Models;
using Nostromo.Server.Database;

namespace Nostromo.Server.Services;

public interface IGenreSyncService
{
    Task SyncGenresAsync(CancellationToken ct);
}

public sealed class GenreSyncService : IGenreSyncService
{
    private readonly IServiceProvider _sp;
    private readonly ITmdbService _tmdb;
    private readonly IDatabaseService _dbService;
    private readonly ILogger<GenreSyncService> _log;

    public GenreSyncService(
        IServiceProvider sp,
        ITmdbService tmdb,
        IDatabaseService dbService,
        ILogger<GenreSyncService> log)
    {
        _sp = sp;
        _tmdb = tmdb;
        _dbService = dbService;
        _log = log;
    }

    public async Task SyncGenresAsync(CancellationToken ct)
    {
        try
        {
            var genreDict = await _tmdb.GetGenreDictionary().ConfigureAwait(false);

            await using var scope = _sp.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<NostromoDbContext>();

            var existing = await db.Genres.AsNoTracking().ToListAsync(ct);

            foreach (var (tmdbId, name) in genreDict)
            {
                var match = existing.FirstOrDefault(g => g.GenreID == tmdbId);

                if (match is null)
                {
                    await _dbService.InsertGenreAsync(new TmdbGenre { id = tmdbId, name = name });
                }
                else if (match.Name != name)
                {
                    match.Name = name;
                    db.Update(match);
                }
            }

            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Genre sync failed");
        }
    }
}