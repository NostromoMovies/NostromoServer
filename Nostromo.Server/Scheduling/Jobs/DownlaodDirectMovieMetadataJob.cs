using Microsoft.Extensions.Logging;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Nostromo.Server.Scheduling.Jobs;
using Nostromo.Server.Services;
using Nostromo.Models;
using Quartz;
using System;
using System.Threading.Tasks;

[DisallowConcurrentExecution]
public class DownloadDirectMovieMetadataJob : BaseJob
{
    private readonly ILogger<DownloadDirectMovieMetadataJob> _logger;
    private readonly IDatabaseService _databaseService;
    private readonly IMovieRepository _movieRepository;
    private readonly ITmdbService _tmdbService;

    public static readonly string HASH_KEY = "FileHash";

    public DownloadDirectMovieMetadataJob(
        ILogger<DownloadDirectMovieMetadataJob> logger,
        IDatabaseService databaseService,
        IMovieRepository movieRepository,
        ITmdbService tmdbService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
        _tmdbService = tmdbService ?? throw new ArgumentNullException(nameof(tmdbService));
    }

    public override string Name => "Download Direct Movie Metadata Job";
    public override string Type => "MovieMetadataDirect";

    public override async Task ProcessJob()
    {
        var fileHash = Context.JobDetail.JobDataMap.GetString(HASH_KEY);
        if (string.IsNullOrWhiteSpace(fileHash))
        {
            _logger.LogError("File hash is missing or invalid for metadata download.");
            return;
        }

        _logger.LogInformation("Starting direct metadata download for file hash: {FileHash}", fileHash);

        try
        {
            var videoId = await _databaseService.GetVideoIdByHashAsync(fileHash);
            if (videoId == null)
            {
                _logger.LogWarning("No video found in Videos table for hash: {FileHash}", fileHash);
                return;
            }

            var createdAt = await _databaseService.GetCreatedAtByVideoIdAsync(videoId);
            _logger.LogInformation("Found VideoID {VideoID} for hash: {FileHash}", videoId, fileHash);

            var movieId = Context.JobDetail.JobDataMap.GetInt("TMDBMovieID");

            _logger.LogInformation("Found MovieID {MovieID} for VideoID {VideoID}", movieId, videoId);

            var movieDetails = await _databaseService.GetMovieAsync(movieId);
            if (movieDetails == null)
            {
                _logger.LogWarning("Movie details not found for MovieID: {MovieID}", movieId);
                return;
            }

            await _databaseService.InsertExampleHashAsync(fileHash, movieId, movieDetails.Title);
            _logger.LogInformation("Inserted ExampleHash entry: {FileHash}, {MovieID}, {Title}", fileHash, movieId, movieDetails.Title);

            int retryCount = 0;
            while (retryCount < 10)
            {
                await Task.Delay(500);
                var verifyInsertion = await _databaseService.GetMovieIdByHashAsync(fileHash);
                if (verifyInsertion != null)
                {
                    _logger.LogInformation("Confirmed ExampleHash insertion for {Hash}", fileHash);
                    break;
                }
                retryCount++;
            }

            try
            {
                var creditsWrapper = await _tmdbService.GetMovieCreditsAsync(movieId);
                if (creditsWrapper?.Cast != null)
                {
                    await _databaseService.StoreMovieCastAsync(movieId, creditsWrapper.Cast);
                    _logger.LogInformation("Stored cast for movie ID {MovieId}", movieId);
                    await _databaseService.StoreMovieCrewAsync(movieId, creditsWrapper.Crew);
                    _logger.LogInformation("Stored crew for movie ID {MovieId}", movieId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing movie cast for ID {MovieId}", movieId);
            }

            var crossRef = new CrossRefVideoTMDBMovie
            {
                TMDBMovieID = movieId,
                VideoID = videoId.Value,
                CreatedAt = createdAt
            };

            await _databaseService.InsertCrossRefAsync(crossRef);
            _logger.LogInformation("Linked TMDBMovieID {TMDBMovieID} to VideoID {VideoID}", movieId, videoId);

            await _databaseService.MarkVideoAsRecognizedAsync(videoId);
            _logger.LogInformation("Marked VideoID {VideoID} as recognized.", videoId);

            if (movieDetails.Genres != null && movieDetails.Genres.Any())
            {
                var tmdbGenres = movieDetails.Genres
                    .Select(g => new TmdbGenre { id = g.GenreID, name = g.Name })
                    .ToList();

                await _databaseService.StoreMovieGenresAsync(movieId, tmdbGenres);

                _logger.LogInformation("Stored {Count} genres for MovieID {MovieID}", tmdbGenres.Count, movieId);
            }
            else
            {
                _logger.LogWarning("Fetching and storing genres for MovieID {MovieID}", movieId);
                var newGenres = await _tmdbService.GetGenresForMovie(movieId);
                await _databaseService.StoreMovieGenresAsync(movieId, newGenres.genres);
            }

            try
            {
                var recommendationsResponse = await _tmdbService.GetRecommendation(movieId);
                if (recommendationsResponse != null && recommendationsResponse.Results.Any())
                {
                    foreach (var recommendation in recommendationsResponse.Results)
                    {
                        await _databaseService.StoreTmdbRecommendationsAsync(movieId, recommendation);
                    }

                    _logger.LogInformation("Successfully stored {Count} recommendations for MovieID {MovieID}",
                        recommendationsResponse.Results.Count, movieId);
                }
                else
                {
                    _logger.LogWarning("No recommendations found for MovieID: {MovieID}", movieId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recommendations for MovieID {MovieID}", movieId);
            }
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

// using Microsoft.Extensions.Logging;
// using Nostromo.Server.Database.Repositories;
// using Nostromo.Server.Database;
// using Nostromo.Server.Scheduling.Jobs;
// using Nostromo.Server.Services;
// using Quartz;
// using System;
// using System.Threading.Tasks;
//
// [DisallowConcurrentExecution]
// public class DownloadDirectMovieMetadataJob : BaseJob
// {
//     private readonly ILogger<DownloadDirectMovieMetadataJob> _logger;
//     private readonly IDatabaseService _databaseService;
//     private readonly IMovieRepository _movieRepository;
//     private readonly ITmdbService _tmdbService;
//
//     public static readonly string HASH_KEY = "FileHash";
//
//     public DownloadDirectMovieMetadataJob(
//         ILogger<DownloadDirectMovieMetadataJob> logger,
//         IDatabaseService databaseService,
//         IMovieRepository movieRepository,
//         ITmdbService tmdbService)
//     {
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//         _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
//         _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
//         _tmdbService = tmdbService ?? throw new ArgumentNullException(nameof(tmdbService));
//     }
//
//     public override string Name => "Download Direct Movie Metadata Job";
//     public override string Type => "MovieMetadataDirect";
//
//     public override async Task ProcessJob()
//     {
//         //get hash value
//         var fileHash = Context.JobDetail.JobDataMap.GetString(HASH_KEY);
//         
//         //check if hash value is null or empty, if it is -- break
//         if (string.IsNullOrWhiteSpace(fileHash))
//         {
//             _logger.LogError("File hash is missing or invalid for metadata download.");
//             return;
//         }
//
//         _logger.LogInformation("Starting direct metadata download for file hash: {FileHash}", fileHash);
//
//         try
//         {
//             
//             var videoId = await _databaseService.GetVideoIdByHashAsync(fileHash);
//             if (videoId == null)
//             {
//                 _logger.LogWarning("No video found in Videos table for hash: {FileHash}", fileHash);
//                 return;
//             }
//
//             var createdAt = await _databaseService.GetCreatedAtByVideoIdAsync(videoId);
//             _logger.LogInformation("Found VideoID {VideoID} for hash: {FileHash}", videoId, fileHash);
//
//             var movieId = Context.JobDetail.JobDataMap.GetInt("TMDBMovieID");
//
//             _logger.LogInformation("Found MovieID {MovieID} for VideoID {VideoID}", movieId, videoId);
//
//             var movieDetails = await _databaseService.GetMovieAsync(movieId);
//             if (movieDetails == null)
//             {
//                 _logger.LogWarning("Movie details not found for MovieID: {MovieID}", movieId);
//                 return;
//             }
//
//             await _databaseService.InsertExampleHashAsync(fileHash, movieId, movieDetails.Title);
//             _logger.LogInformation("Inserted ExampleHash entry: {FileHash}, {MovieID}, {Title}", fileHash, movieId, movieDetails.Title);
//
//             int retryCount = 0;
//             while (retryCount < 10)
//             {
//                 await Task.Delay(500);
//                 var (verifyInsertion, _, _) = await _databaseService.GetMovieIdByHashAsync(fileHash);
//                 
//                 if (verifyInsertion != null)
//                 {
//                     _logger.LogInformation("Confirmed ExampleHash insertion for {Hash}", fileHash);
//                     break;
//                 }
//                 retryCount++;
//             }
//
//             try
//             {
//                 var creditsWrapper = await _tmdbService.GetMovieCreditsAsync(movieId);
//                 if (creditsWrapper?.Cast != null)
//                 {
//                     await _databaseService.StoreMovieCastAsync(movieId, creditsWrapper.Cast, true, false, false);
//                     _logger.LogInformation("Stored cast for movie ID {MovieId}", movieId);
//                     await _databaseService.StoreMovieCrewAsync(movieId, creditsWrapper.Crew, true, false, false);
//                     _logger.LogInformation("Stored crew for movie ID {MovieId}", movieId);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error processing movie cast for ID {MovieId}", movieId);
//             }
//
//             var crossRef = new CrossRefVideoTMDBMovie
//             {
//                 TMDBMovieID = movieId,
//                 VideoID = videoId.Value,
//                 CreatedAt = createdAt
//             };
//
//             await _databaseService.InsertCrossRefAsync(crossRef);
//             _logger.LogInformation("Linked TMDBMovieID {TMDBMovieID} to VideoID {VideoID}", movieId, videoId);
//
//             await _databaseService.MarkVideoAsRecognizedAsync(videoId);
//             _logger.LogInformation("Marked VideoID {VideoID} as recognized.", videoId);
//
//             try
//             {
//                 var recommendationsResponse = await _tmdbService.GetRecommendation(movieId);
//                 if (recommendationsResponse != null && recommendationsResponse.Results.Any())
//                 {
//                     foreach (var recommendation in recommendationsResponse.Results)
//                     {
//                         await _databaseService.StoreTmdbRecommendationsAsync(movieId, recommendation);
//                     }
//
//                     _logger.LogInformation("Successfully stored {Count} recommendations for MovieID {MovieID}",
//                         recommendationsResponse.Results.Count, movieId);
//                 }
//                 else
//                 {
//                     _logger.LogWarning("No recommendations found for MovieID: {MovieID}", movieId);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error processing recommendations for MovieID {MovieID}", movieId);
//             }
//         }
//         catch (NotFoundException ex)
//         {
//             _logger.LogWarning("Movie ID not found on TMDB: {Message}", ex.Message);
//             return;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Unhandled error processing metadata for file hash: {FileHash}", fileHash);
//             throw;
//         }
//     }
// }
