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
public class DownloadDirectTvMetadataJob : BaseJob
{
    private readonly ILogger<DownloadDirectTvMetadataJob> _logger;
    private readonly IDatabaseService _databaseService;
    private readonly ISeasonRepository _seasonRepository;
    private readonly ITmdbService _tmdbService;

    public static readonly string HASH_KEY = "FileHash";
    public static readonly string SHOW_ID_KEY = "TMDBShowID";
    public static readonly string SEASON_NUM_KEY = "SeasonNumber";
    public static readonly string EPISODE_NUM_KEY = "EpisodeNumber";

    public DownloadDirectTvMetadataJob(
        ILogger<DownloadDirectTvMetadataJob> logger,
        IDatabaseService databaseService,
        ISeasonRepository seasonRepository,
        ITmdbService tmdbService)
    {
        _logger = logger;
        _databaseService = databaseService;
        _seasonRepository = seasonRepository;
        _tmdbService = tmdbService;
    }

    public override string Name => "Download Direct TV Metadata Job";
    public override string Type => "TvMetadataDirect";

    public override async Task ProcessJob()
    {
        var fileHash = Context.JobDetail.JobDataMap.GetString(HASH_KEY);
        var showId = Context.JobDetail.JobDataMap.GetInt(SHOW_ID_KEY);
        var seasonNumber = Context.JobDetail.JobDataMap.GetInt(SEASON_NUM_KEY);
        var episodeNumber = Context.JobDetail.JobDataMap.GetInt(EPISODE_NUM_KEY);

        if (string.IsNullOrWhiteSpace(fileHash) || showId <= 0 || seasonNumber < 0 || episodeNumber < 0)
        {
            _logger.LogError("Invalid parameters provided to job.");
            return;
        }

        var videoId = await _databaseService.GetVideoIdByHashAsync(fileHash);
        var createdAt = await _databaseService.GetCreatedAtByVideoIdAsync(videoId);

        if (videoId == null)
        {
            _logger.LogWarning("No video found for hash {Hash}", fileHash);
            return;
        }

        var seasonId = await _seasonRepository.GetSeasonIdAsync(showId, seasonNumber);
        if (seasonId == null)
        {
            _logger.LogWarning("No season found for Show {ShowID}, Season {SeasonNumber}", showId, seasonNumber);
            return;
        }

        var episode = await _tmdbService.GetTvEpisodeById(showId, seasonNumber, episodeNumber, seasonId.Value);
        var episodeId = episode.EpisodeID;

        await _databaseService.InsertTvExampleHashAsync(fileHash, showId, episode.EpisodeName, seasonNumber, episodeNumber);
        await _databaseService.InsertTvCrossRefAsync(new CrossRefVideoTvEpisode
        {
            TvEpisodeId = episodeId,
            VideoID = videoId.Value,
            CreatedAt = createdAt
        });

        await _databaseService.MarkVideoAsRecognizedAsync(videoId);

        var showCredits = await _tmdbService.GetTvShowCreditsAsync(showId);
        var episodeCredits = await _tmdbService.GetTvEpisodeCreditsAsync(showId, seasonNumber, episodeNumber);

        await _databaseService.StoreTvMediaCastAsync(showId, showCredits.Cast);
        await _databaseService.StoreTvMediaCrewAsync(showId, showCredits.Crew);

        await _databaseService.StoreTvMediaCastAsync(episodeId, episodeCredits.Cast);
        await _databaseService.StoreTvMediaCastAsync(episodeId, episodeCredits.GuestStars);
        await _databaseService.StoreTvMediaCrewAsync(episodeId, episodeCredits.Crew);

        _logger.LogInformation("TV episode metadata successfully stored for ShowID {ShowID}, S{Season}, E{Episode}",
            showId, seasonNumber, episodeNumber);
    }
}