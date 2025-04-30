using Microsoft.Extensions.Logging;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Nostromo.Server.Scheduling.Jobs;
using Nostromo.Server.Services;
using Nostromo.Models;
using Quartz;
using System;

[DisallowConcurrentExecution]
public class DownloadTvMetadataJob : BaseJob
{
    private readonly ILogger<DownloadTvMetadataJob> _logger;
    private readonly IDatabaseService _databaseService;
    private readonly ITvShowRepository _tvShowRepository;
    private readonly ISeasonRepository _seasonRepository;
    private readonly ITvEpisodeRepository _tvEpisodeRepository;
    private readonly ITmdbService _tmdbService;

    public static readonly string HASH_KEY = "FileHash";

    public DownloadTvMetadataJob(
        ILogger<DownloadTvMetadataJob> logger,
        IDatabaseService databaseService,
        ITvShowRepository tvShowRepository,
        ISeasonRepository seasonRepository,
        ITvEpisodeRepository tvEpisodeRepository,
        ITmdbService tmdbService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _tvShowRepository = tvShowRepository ?? throw new ArgumentNullException(nameof(tvShowRepository));
        _seasonRepository = seasonRepository ?? throw new ArgumentNullException(nameof(seasonRepository));
        _tvEpisodeRepository = tvEpisodeRepository ?? throw new ArgumentNullException(nameof(tvEpisodeRepository));
        _tmdbService = tmdbService ?? throw new ArgumentNullException(nameof(tmdbService));
    }

    public override string Name => "Download Tv Metadata Job";
    public override string Type => "TvMetadata";

    public override async Task ProcessJob()
    {
        var fileHash = Context.JobDetail.JobDataMap.GetString(HASH_KEY);
        int episodeID = 0;
        if (string.IsNullOrWhiteSpace(fileHash))
        {
            _logger.LogError("File hash is missing or invalid for tv metadata download.");
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

            // Step 2: Retrieve TvShowID, SeasonNumber and EpisodeNumber from hash
            var (showId, seasonNumber, episodeNumber) = await _databaseService.GetTvShowIdSeasonEpByHashAsync(fileHash);
            if (showId == null)
            {
                _logger.LogWarning("No Tv Show found in ExampleHashes table for hash: {FileHash}", fileHash);
                await _databaseService.MarkVideoAsUnrecognizedAsync(videoId);
                return;
            }

            _logger.LogInformation("Found ShowID {ShowID} for hash: {FileHash}", showId, fileHash);

            try
            {
                // Step 3: Fetch metadata from TMDB and save to database
                var showResponse = await _tmdbService.GetTvShowById(showId.Value);
                var seasonId = await _seasonRepository.GetSeasonIdAsync(showId.Value, seasonNumber.Value);
                if (seasonId == null)
                {
                    _logger.LogInformation("SeasonId is not available to fetch episode");
                    return;
                }
                
                var episodeResponse = await _tmdbService.GetTvEpisodeById(showId.Value, seasonNumber.Value, episodeNumber.Value, seasonId.Value);
                episodeID = episodeResponse.EpisodeID;
                _logger.LogInformation("Tv metadata saved to database for TvShowId: {ShowID}", showId.Value);

                var certification = await _tmdbService.GetTvCertificationAsync(showId.Value);
                if (certification != null)
                {
                    await _databaseService.UpdateTvCertificationAsync(showId.Value, certification);
                    _logger.LogInformation("Stored certification '{Certification}' for show ID {ShowId}", certification, showId);
                }
                
                try
                {
                    var showCreditsWrapper = await _tmdbService.GetTvShowCreditsAsync(showId.Value);
                    if (showCreditsWrapper?.Cast != null)
                    {
                        await _databaseService.StoreTvMediaCastAsync(showId.Value, showCreditsWrapper.Cast);
                        _logger.LogInformation("Successfully processed and stored cast for show ID {ShowId}", showId);
                        await _databaseService.StoreTvMediaCrewAsync(showId.Value, showCreditsWrapper.Crew);
                        _logger.LogInformation("Successfully processed and stored crew for show ID {ShowId}", showId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing show cast for ID {ShowId}", showId);
                }

                try
                {
                    var episodeCreditsWrapper = await _tmdbService.GetTvEpisodeCreditsAsync(showId.Value, seasonNumber.Value, episodeNumber.Value);
                    if (episodeCreditsWrapper?.Cast != null)
                    {
                        await _databaseService.StoreTvMediaCastAsync(episodeID, episodeCreditsWrapper.Cast);
                        _logger.LogInformation("Successfully processed and stored episode cast for show ID {ShowId}", showId);

                        await _databaseService.StoreTvMediaCastAsync(episodeID, episodeCreditsWrapper.GuestStars);
                        _logger.LogInformation("Successfully processed and stored episode guest cast for show ID {ShowId}", showId);
                        
                        await _databaseService.StoreTvMediaCrewAsync(episodeID, episodeCreditsWrapper.Crew);
                        _logger.LogInformation("Successfully processed and stored episode crew for show ID {ShowId}", showId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing episode cast for ShowID {ShowId}", showId);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching show metadata for ShowID: {ShowID}", showId);
                return;
            }

            // Step 4: Save cross-reference entry
            var crossRef = new CrossRefVideoTvEpisode
            {
                TvEpisodeId = episodeID,
                VideoID = videoId.Value,
                CreatedAt = createdAt
            };
            
            await _databaseService.InsertTvCrossRefAsync(crossRef);
            _logger.LogInformation("Linked ShowId {ShowId} to VideoID {VideoID}", showId, videoId);
            
            await _databaseService.MarkVideoAsRecognizedAsync(videoId);
            _logger.LogInformation("Marked VideoID {VideoID} as recognized.", videoId);

            // Step 5: Fetch and Store Recommendations
            try
            {
                var recommendationsResponse = await _tmdbService.GetTvRecommendation(showId.Value);
                if (recommendationsResponse != null && recommendationsResponse.Results.Any())
                {
                    var GenreDict = await _databaseService.GetGenreDictionary();
                    
                    foreach (var recommendation in recommendationsResponse.Results)
                    {
                        await _databaseService.StoreTvRecommendationsAsync(showId.Value, recommendation, GenreDict);
                        
                    }

                    foreach (var recommendation in recommendationsResponse.Results)
                    {
                        var certification = await _tmdbService.GetTvCertificationAsync(recommendation.Id);
                        if (certification != null)
                        {
                            await _databaseService.UpdateTvRecommendationCertificationAsync(recommendation.Id, certification);
                            _logger.LogInformation("Stored certification '{Certification}' for show ID {ShowId}", certification, recommendation.Id);
                        }
                        else
                        {
                            _logger.LogInformation("Error Storing certification '{Certification}' for show ID {ShowId}", certification, recommendation.Id);
                        }
                    }

                    _logger.LogInformation("Successfully stored {Count} recommendations for ShowId {ShowId}",
                        recommendationsResponse.Results.Count, showId);
                }
                else
                {
                    _logger.LogWarning("No recommendations found for ShowId: {ShowId}", showId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching or storing TvShow recommendations for ShowId: {ShowId}", showId);
            }
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Show ID not found on TvShow: {Message}", ex.Message);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing metadata for file hash: {FileHash}", fileHash);
            throw;
        }
    }
}
