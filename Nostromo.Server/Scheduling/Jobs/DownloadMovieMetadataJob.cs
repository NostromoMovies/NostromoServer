using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Nostromo.Server.Services;
using Nostromo.Models;
using Nostromo.Server.Database;

namespace Nostromo.Server.Scheduling.Jobs
{
    [DisallowConcurrentExecution]
    public class DownloadMovieMetadataJob : BaseJob
    {
        public static readonly string HASH_KEY = "FileHash";
        public static readonly string TMDB_API_KEY = "TmdbKey";
        public static readonly string TMDB_BASE_URL = "TmdbBaseUrl";

        private readonly ILogger<DownloadMovieMetadataJob> _logger;
        private readonly HttpClient _httpClient;
        private readonly IDatabaseService _databaseService;
        private readonly string _tmdbApiKey;
        private readonly string _tmdbBaseUrl;

        public DownloadMovieMetadataJob(
            ILogger<DownloadMovieMetadataJob> logger,
            HttpClient httpClient,
            IDatabaseService databaseService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        public override string Name => "Download Movie Metadata Job";
        public override string Type => "MovieMetadata";

        // This is where the job's main logic is executed
        public override async Task ProcessJob()
        {
            // Retrieve the file hash from the JobDataMap using the constant key
            var fileHash = Context.MergedJobDataMap.GetString(HASH_KEY);
            if (string.IsNullOrWhiteSpace(fileHash))
            {
                _logger.LogError("File hash is missing or invalid for metadata download.");
                return;
            }

            // Retrieve the TMDB API key and base URL from the JobDataMap
            var tmdbApiKey = Context.MergedJobDataMap.GetString(TMDB_API_KEY);
            var tmdbBaseUrl = Context.MergedJobDataMap.GetString(TMDB_BASE_URL);

            // Log the start of the metadata download process
            _logger.LogInformation("Starting metadata download for file hash: {FileHash}", fileHash);

            try
            {
                // Step 1: Retrieve VideoID from hash
                var videoId = await _databaseService.GetVideoIdByHashAsync(fileHash);
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
                    return;
                }

                _logger.LogInformation("Found MovieID {MovieID} for hash: {FileHash}", movieId, fileHash);

                // Step 3: Fetch metadata from TMDB
                var movieUrl = $"{tmdbBaseUrl}/movie/{movieId}?api_key={tmdbApiKey}";
                var movie = await _httpClient.GetFromJsonAsync<TmdbMovie>(movieUrl);

                if (movie == null)
                {
                    _logger.LogWarning("Movie with ID {MovieID} not found on TMDB.", movieId);
                    return;
                }

                // Step 4: Save movie metadata to the database
                try
                {
                    await _databaseService.InsertMovieAsync(movie);
                    _logger.LogInformation("Movie metadata saved to database for MovieID: {MovieID}", movieId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inserting movie metadata into database for MovieID: {MovieID}", movieId);
                    return;
                }

                // Step 5: Save cross-reference entry
                try
                {
                    var crossRef = new CrossRefVideoTMDBMovie
                    {
                        TMDBMovieID = movieId.Value,
                        VideoID = videoId.Value
                    };

                    await _databaseService.InsertCrossRefAsync(crossRef);
                    _logger.LogInformation("Linked TMDBMovieID {TMDBMovieID} to VideoID {VideoID}", movieId, videoId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating cross-reference for TMDBMovieID: {TMDBMovieID} and VideoID: {VideoID}", movieId, videoId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error processing metadata for file hash: {FileHash}", fileHash);
            }
        }
    }
}
