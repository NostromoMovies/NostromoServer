// !!!
// PROBABLY NOT NECESSARY TO USE THIS BUT LEAVING IT ANYWAY
// !!!
using Microsoft.Extensions.Logging;
using Nostromo.Server.Scheduling.Jobs;
using Nostromo.Server.Services;

public class DownloadTMDBMetadataJob : BaseJob
{
    private readonly ILogger<DownloadTMDBMetadataJob> _logger;
    private readonly ITmdbService _tmdbService;
    private readonly IDatabaseService _databaseService;

    public const string MOVIE_ID_KEY = "MovieId";
    public override string Name => "Download TMDB Metadata Job";
    public override string Type => "Metadata";

    public DownloadTMDBMetadataJob(
        ILogger<DownloadTMDBMetadataJob> logger,
        ITmdbService tmdbService,
        IDatabaseService databaseService)
    {
        _logger = logger;
        _tmdbService = tmdbService;
        _databaseService = databaseService;
    }

    public override async Task ProcessJob()
    {
        var jobDataMap = Context.MergedJobDataMap;
        var movieId = jobDataMap.GetInt(MOVIE_ID_KEY);
        try
        {
            _logger.LogInformation("Starting metadata download for movie {MovieId}", movieId);

            var movie = await _tmdbService.GetMovieById(movieId);

            _logger.LogInformation("Successfully downloaded metadata for movie {MovieId}: {Title}",
                movieId, movie.title);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Movie {MovieId} not found in TMDB", movieId);
            // Handle not found case
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing metadata download job for movie {MovieId}", movieId);
            throw; // Rethrow to mark job as failed
        }
    }
}