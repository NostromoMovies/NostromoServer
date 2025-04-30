using Microsoft.Extensions.Logging;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Nostromo.Server.Scheduling.Jobs;
using Nostromo.Server.Services;
using Nostromo.Models;
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

                var certification = await _tmdbService.GetCertificationAsync(movieId.Value);
                if (certification != null)
                {
                    await _databaseService.UpdateMovieCertificationAsync(movieId.Value, certification);
                    _logger.LogInformation("Stored certification '{Certification}' for movie ID {MovieId}", certification, movieId);
                }

                if (movieResponse.genreIds != null && movieResponse.genreIds.Any())
                {
                    await _databaseService.StoreMovieGenresAsync(movieId.Value, movieResponse.genreIds);

                    _logger.LogInformation("Stored {Count} genres for MovieID {MovieID}", movieResponse.genreIds.Count, movieId);
                }

                else
                {
                    _logger.LogInformation("No genres for MovieID {MovieID}", movieId.Value);
                }

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

            // Step 5: Fetch and Store Recommendations
            try
            {
                var recommendationsResponse = await _tmdbService.GetRecommendation(movieId.Value);
                if (recommendationsResponse != null && recommendationsResponse.Results.Any())
                {
                    foreach (var recommendation in recommendationsResponse.Results)
                    {
                        await _databaseService.StoreTmdbRecommendationsAsync(movieId.Value, recommendation);
                    }

                    foreach (var recommendation in recommendationsResponse.Results)
                    {
                        var genres = await _tmdbService.GetGenresForMovie(recommendation.id);
                        var recDbId = await _databaseService.GetActualRecommendationDbIdAsync(movieId.Value, recommendation.id);

                        if (recDbId != null)
                        {
                            foreach (var genre in genres.genres)
                            {
                                await _databaseService.InsertRecommendationGenreAsync(recDbId.Value, genre.id, genre.name);
                            }
                        }
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
                _logger.LogError(ex, "Error fetching or storing movie recommendations for MovieID: {MovieID}", movieId);
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
// using System.Text.Json;
// using NHibernate.Hql.Ast;
//
// [DisallowConcurrentExecution]
// public class DownloadMovieMetadataJob : BaseJob
// {
//     private readonly ILogger<DownloadMovieMetadataJob> _logger;
//     private readonly IDatabaseService _databaseService;
//     private readonly IMovieRepository _movieRepository;
//     private readonly ITmdbService _tmdbService;
//     private readonly ISeasonRepository _seasonRepository;
//
//     public static readonly string HASH_KEY = "FileHash";
//
//     public DownloadMovieMetadataJob(
//         ILogger<DownloadMovieMetadataJob> logger,
//         IDatabaseService databaseService,
//         IMovieRepository movieRepository,
//         ITmdbService tmdbService,
//         ISeasonRepository seasonRepository)
//
//     {
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//         _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
//         _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
//         _tmdbService = tmdbService ?? throw new ArgumentNullException(nameof(tmdbService));
//         _seasonRepository = seasonRepository ?? throw new ArgumentNullException(nameof(seasonRepository));
//     }
//
//     public override string Name => "Download Movie Metadata Job";
//
//     public override string Type => "MovieMetadata";
//
//     public override async Task ProcessJob()
//     {
//         //get hash value
//         var fileHash = Context.JobDetail.JobDataMap.GetString(HASH_KEY);
//
//         //check if hash is null or empty, if true --break
//
//         if (string.IsNullOrWhiteSpace(fileHash))
//         {
//
//             _logger.LogError("File hash is missing or invalid for metadata download.");
//
//             return;
//
//         }
//
//         _logger.LogInformation("Starting metadata download for file hash: {FileHash}", fileHash);
//
//         try
//         {
//             // Step 1: Retrieve VideoID from hash
//             var videoId = await _databaseService.GetVideoIdByHashAsync(fileHash);
//             var createdAt = await _databaseService.GetCreatedAtByVideoIdAsync(videoId);
//
//             //if videoID is null --break
//             if (videoId == null)
//             {
//                 _logger.LogWarning("No video found in Videos table for hash: {FileHash}", fileHash);
//                 return;
//             }
//
//             _logger.LogInformation("Found VideoID {VideoID} for hash: {FileHash}", videoId, fileHash);
//
//
//             // Step 2: Retrieve tmdbID from hash
//
//             var (mediaID, episodeNum, seasonNum) = await _databaseService.GetMovieIdByHashAsync(fileHash);
//
//             //if mediaID is null --break
//             if (mediaID == null)
//             {
//                 _logger.LogWarning("No movie or tv show found in ExampleHashes table for hash: {FileHash}", fileHash);
//                 await _databaseService.MarkVideoAsUnrecognizedAsync(videoId);
//                 return;
//             }
//
//             if (episodeNum == null && seasonNum == null)
//             {
//                 _logger.LogInformation("Found MovieID {MovieID} for hash: {FileHash}", mediaID, fileHash);
//
//                 try
//                 {
//
//                     // Step 3: Fetch metadata from TMDB and save to database
//                     var movieResponse = await _tmdbService.GetMovieById(mediaID.Value);
//
//                     _logger.LogInformation("Movie metadata saved to database for MovieID: {MovieID}", mediaID);
//
//                     try
//                     {
//                         var creditsWrapper = await _tmdbService.GetMovieCreditsAsync(mediaID.Value);
//
//                         if (creditsWrapper?.Cast != null)
//                         {
//                             await _databaseService.StoreMovieCastAsync(mediaID.Value, creditsWrapper.Cast, true, false, false);
//
//                             _logger.LogInformation("Successfully processed and stored cast for movie ID {MovieId}", mediaID);
//
//                             await _databaseService.StoreMovieCrewAsync(mediaID.Value, creditsWrapper.Crew, true, false, false);
//
//                             _logger.LogInformation("Successfully processed and stored crew for movie ID {MovieId}", mediaID);
//
//                         }
//
//                     }
//
//                     catch (Exception ex)
//                     {
//
//                         _logger.LogError(ex, "Error processing movie cast for ID {MovieId}", mediaID);
//
//                     }
//
//                 }
//
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Error fetching movie metadata for MovieID: {MovieID}", mediaID);
//
//                     return;
//                 }
//
//                 // Step 4: Save cross-reference entry
//                 var crossRef = new CrossRefVideoTMDBMovie
//                 {
//                     TMDBMovieID = mediaID.Value,
//                     VideoID = videoId.Value,
//                     CreatedAt = createdAt
//                 };
//
//                 await _databaseService.InsertCrossRefAsync(crossRef);
//
//                 _logger.LogInformation("Linked TMDBMovieID {TMDBMovieID} to VideoID {VideoID}", mediaID, videoId);
//
//                 await _databaseService.MarkVideoAsRecognizedAsync(videoId);
//
//                 _logger.LogInformation("Marked VideoID {VideoID} as recognized.", videoId);
//
//                 // Step 5: Fetch and Store Recommendations
//
//                 try
//                 {
//                     var recommendationsResponse = await _tmdbService.GetRecommendation(mediaID.Value);
//
//                     if (recommendationsResponse != null && recommendationsResponse.Results.Any())
//                     {
//
//                         foreach (var recommendation in recommendationsResponse.Results)
//                         {
//                             await _databaseService.StoreTmdbRecommendationsAsync(mediaID.Value, recommendation);
//
//                         }
//
//                         _logger.LogInformation("Successfully stored {Count} recommendations for MovieID {MovieID}",
//                             recommendationsResponse.Results.Count, mediaID);
//
//                     }
//
//                     else
//                     {
//
//                         _logger.LogWarning("No recommendations found for MovieID: {MovieID}", mediaID);
//
//                     }
//
//                 }
//
//                 catch (Exception ex)
//                 {
//
//                     _logger.LogError(ex, "Error fetching or storing movie recommendations for MovieID: {MovieID}",
//                         mediaID);
//
//                 }
//
//             }
//
//
//
//             else
//             {
//                 _logger.LogInformation("Found TvShowID {ShowID} for hash: {FileHash}", mediaID, fileHash);
//
//                 var episodeIdHolder = 0;
//
//                 try
//                 {
//
//                     // Step 3: Fetch metadata from TMDB and save to database
//
//                     var tvShowResponse = await _tmdbService.GetTvShowById(mediaID.Value);
//
//                     var seasonId = await _seasonRepository.GetSeasonIdAsync(mediaID.Value, seasonNum.Value);
//
//                     var episodeResponse = await _tmdbService.GetTvEpisodeById(mediaID.Value, seasonNum.Value, episodeNum.Value, seasonId.Value);
//
//                     _logger.LogInformation("Tv Show metadata saved to database for TvShowID: {ShowID}", mediaID);
//
//                     episodeIdHolder = episodeResponse.EpisodeID;
//
//                     try
//                     {
//
//                         var epCreditsWrapper = await _tmdbService.GetTvEpisodeCreditsAsync(mediaID.Value, seasonNum.Value, episodeNum.Value);
//                         
//                         var showCreditsWrapper = await _tmdbService.GetTvShowCreditsAsync(mediaID.Value);
//                         
//                         _logger.LogInformation("Episode Credits for Show ID {ShowId}, Season {Season}, Episode {Episode}: {@EpisodeCredits}", mediaID.Value, seasonNum.Value, episodeNum.Value, epCreditsWrapper);
//                         _logger.LogInformation("Show Credits for Show ID {ShowId}: {@ShowCredits}", mediaID.Value, showCreditsWrapper);
//                         
//                         _logger.LogInformation("Episode Crew (First 10):\n{Crew}",
//                             JsonSerializer.Serialize(epCreditsWrapper?.Crew?.Take(10), new JsonSerializerOptions { WriteIndented = true }));
//
//                         _logger.LogInformation("Show Crew (First 10):\n{Crew}",
//                             JsonSerializer.Serialize(showCreditsWrapper?.Crew?.Take(10), new JsonSerializerOptions { WriteIndented = true }));
//
//
//                         if (epCreditsWrapper?.Cast != null)
//                         {
//
//                             await _databaseService.StoreMovieCastAsync(mediaID.Value, showCreditsWrapper.Cast, false, true, false);
//
//                             _logger.LogInformation("Successfully processed and stored cast for Tv Show ID {ShowId}",
//                                 mediaID.Value);
//
//                             await _databaseService.StoreMovieCastAsync(episodeIdHolder, epCreditsWrapper.Cast, false, false, true);
//
//                             _logger.LogInformation("Successfully processed and stored cast for Tv Episode ID {EpisodeId}",
//                                 episodeIdHolder);
//
//                             await _databaseService.StoreMovieCrewAsync(mediaID.Value, showCreditsWrapper.Crew, false, true, false);
//
//                             _logger.LogInformation("Successfully processed and stored crew for Tv Show ID {ShowId}",
//                                 mediaID.Value);
//
//                             await _databaseService.StoreMovieCrewAsync(episodeIdHolder, epCreditsWrapper.Crew, false, false, true);
//
//                             _logger.LogInformation("Successfully processed and stored crew for Tv Episode ID {EpisodeId}",
//                                 episodeIdHolder);
//
//                         }
//
//                     }
//
//                     catch (Exception ex)
//                     {
//
//                         _logger.LogError(ex, "Error processing Tv Show cast for ID {ShowId}", episodeIdHolder);
//
//                     }
//
//                 }
//
//                 catch (Exception ex)
//                 {
//
//                     _logger.LogError(ex, "Error fetching movie metadata for TvShowId: {ShowId}", mediaID.Value);
//
//                     return;
//
//                 }
//
//
//
//                 // Step 4: Save cross-reference entry
//
//                 var crossRef = new CrossRefVideoTMDBMovie
//                 {
//                     TMDBMovieID = episodeIdHolder,
//                     VideoID = videoId.Value,
//                     CreatedAt = createdAt
//                 };
//
//                 await _databaseService.InsertCrossRefAsync(crossRef);
//
//                 _logger.LogInformation("Linked Episodeid {EpisodeId} to VideoID {VideoID}", mediaID.Value, videoId);
//
//                 await _databaseService.MarkVideoAsRecognizedAsync(videoId);
//
//                 _logger.LogInformation("Marked VideoID {VideoID} as recognized.", videoId);
//
//
//
//                 // Step 5: Fetch and Store Recommendations
//                 try
//                 {
//                     var recommendationsResponse = await _tmdbService.GetTvRecommendation(mediaID.Value);
//
//                     if (recommendationsResponse != null && recommendationsResponse.Results.Any())
//                     {
//
//                         foreach (var recommendation in recommendationsResponse.Results)
//                         {
//
//                             await _databaseService.StoreTvRecommendationsAsync(mediaID.Value, recommendation);
//
//                         }
//
//
//                         _logger.LogInformation("Successfully stored {Count} recommendations for ShowId {ShowId}",
//                             recommendationsResponse.Results.Count, mediaID.Value);
//
//                     }
//
//                     else
//                     {
//
//                         _logger.LogWarning("No recommendations found for ShowID: {ShowId}", mediaID.Value);
//
//                     }
//                 }
//
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Error fetching or storing movie recommendations for MovieID: {MovieID}",
//                         mediaID.Value);
//                 }
//             }
//         }
//
//         catch (NotFoundException ex)
//         {
//             _logger.LogWarning("Movie ID not found on TMDB: {Message}", ex.Message);
//             return;
//         }
//
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Unhandled error processing metadata for file hash: {FileHash}", fileHash);
//             throw;
//         }
//    }
// }