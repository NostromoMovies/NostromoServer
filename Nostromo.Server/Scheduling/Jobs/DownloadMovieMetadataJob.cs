using Microsoft.Extensions.Logging;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Nostromo.Server.Scheduling.Jobs;
using Nostromo.Server.Services;
using Quartz;
using System;

[DisallowConcurrentExecution]
public class DownloadMovieMetadataJob : BaseJob
{
    private readonly ILogger<DownloadMovieMetadataJob> _logger;
    private readonly IDatabaseService _databaseService;
    private readonly IMovieRepository _movieRepository;
    private readonly ITmdbService _tmdbService;

    public static readonly string HASH_KEY = "FileHash";

    public DownloadMovieMetadataJob(
        ILogger<DownloadMovieMetadataJob> logger,
        IDatabaseService databaseService,
        IMovieRepository movieRepository,
        ITmdbService tmdbService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
        _tmdbService = tmdbService ?? throw new ArgumentNullException(nameof(tmdbService));
    }

    public override string Name => "Download Movie Metadata Job";
    public override string Type => "MovieMetadata";

    public override async Task ProcessJob()
    {
        var fileHash = Context.JobDetail.JobDataMap.GetString(HASH_KEY);
        if (string.IsNullOrWhiteSpace(fileHash))
        {
            _logger.LogError("File hash is missing or invalid for metadata download.");
            return;
        }

        _logger.LogInformation("Starting metadata download for file hash: {FileHash}", fileHash);

        try
        {
            // Step 1: Retrieve VideoID from hash
            var videoId = await _databaseService.GetVideoIdByHashAsync(fileHash);
            var createdAt = await _databaseService.GetCreatedAtByVideoIdAsync(videoId);
            if (videoId == null)
            {
                _logger.LogWarning("No video found in Videos table for hash: {FileHash}", fileHash);
                return;
            }

            _logger.LogInformation("Found VideoID {VideoID} for hash: {FileHash}", videoId, fileHash);

            // Step 2: Retrieve MovieID from hash
            var movieId = await _databaseService.GetMovieIdByHashAsync(fileHash);
            if (movieId == null)
            {
                _logger.LogWarning("No movie found in ExampleHashes table for hash: {FileHash}", fileHash);
                await _databaseService.MarkVideoAsUnrecognizedAsync(videoId);
                return;
            }

            _logger.LogInformation("Found MovieID {MovieID} for hash: {FileHash}", movieId, fileHash);

            try
            {
                // Step 3: Fetch metadata from TMDB and save to database
                var movieResponse = await _tmdbService.GetMovieById(movieId.Value);
                _logger.LogInformation("Movie metadata saved to database for MovieID: {MovieID}", movieId);

                try
                {
                    var creditsWrapper = await _tmdbService.GetMovieCreditsAsync(movieId.Value);
                    if (creditsWrapper?.Cast != null)
                    {
                        await _databaseService.StoreMovieCastAsync(movieId.Value, creditsWrapper.Cast);
                        _logger.LogInformation("Successfully processed and stored cast for movie ID {MovieId}", movieId);
                        await _databaseService.StoreMovieCrewAsync(movieId.Value, creditsWrapper.Crew);
                        _logger.LogInformation("Successfully processed and stored crew for movie ID {MovieId}", movieId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing movie cast for ID {MovieId}", movieId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movie metadata for MovieID: {MovieID}", movieId);
                return;
            }

            // Step 4: Save cross-reference entry
            var crossRef = new CrossRefVideoTMDBMovie
            {
                TMDBMovieID = movieId.Value,
                VideoID = videoId.Value,
                CreatedAt = createdAt
            };

            await _databaseService.InsertCrossRefAsync(crossRef);
            _logger.LogInformation("Linked TMDBMovieID {TMDBMovieID} to VideoID {VideoID}", movieId, videoId);

            await _databaseService.MarkVideoAsRecognizedAsync(videoId);
            _logger.LogInformation("Marked VideoID {VideoID} as recognized.", videoId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Movie ID not found on TMDB: {Message}", ex.Message);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing metadata for file hash: {FileHash}", fileHash);
            throw;
        }
    }
}