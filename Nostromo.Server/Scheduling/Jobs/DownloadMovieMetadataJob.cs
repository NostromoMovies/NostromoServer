using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Nostromo.Server.Services;
using Nostromo.Models;

namespace Nostromo.Server.Scheduling.Jobs
{
    [DisallowConcurrentExecution]
    public class DownloadMovieMetadataJob : BaseJob
    {
        private readonly ILogger<DownloadMovieMetadataJob> _logger;
        private readonly HttpClient _httpClient;
        private readonly IDatabaseService _databaseService;
        private readonly string _tmdbApiKey;
        private readonly string _tmdbBaseUrl;

        public static readonly string HASH_KEY = "FileHash";

        public DownloadMovieMetadataJob(
            ILogger<DownloadMovieMetadataJob> logger,
            HttpClient httpClient,
            IDatabaseService databaseService,
            string tmdbApiKey,
            string tmdbBaseUrl)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _tmdbApiKey = tmdbApiKey ?? throw new ArgumentNullException(nameof(tmdbApiKey));
            _tmdbBaseUrl = tmdbBaseUrl ?? throw new ArgumentNullException(nameof(tmdbBaseUrl));
        }

        public override string Name => "Download Movie Metadata Job";
        public override string Type => "MovieMetadata";

        public override async Task ProcessJob()
        {
            // Retrieve the hash from the JobDataMap
            var fileHash = Context.JobDetail.JobDataMap.GetString(HASH_KEY);
            if (string.IsNullOrWhiteSpace(fileHash))
            {
                _logger.LogError("File hash is missing or invalid for metadata download.");
                return;
            }

            _logger.LogInformation("Starting metadata download for file hash: {FileHash}", fileHash);

            try
            {
                // Step 1: Check the hash in the ExampleHashes table
                var movieId = await _databaseService.GetMovieIdByHashAsync(fileHash);
                if (movieId == null)
                {
                    _logger.LogWarning("No movie found in ExampleHashes table for hash: {FileHash}", fileHash);
                    return;
                }

                _logger.LogInformation("Found MovieID {MovieID} for hash: {FileHash}", movieId, fileHash);

                // Step 2: Fetch metadata from TMDB
                var movieUrl = $"{_tmdbBaseUrl}/movie/{movieId}?api_key={_tmdbApiKey}";
                var movie = await _httpClient.GetFromJsonAsync<TmdbMovie>(movieUrl);

                if (movie == null)
                {
                    _logger.LogWarning("Movie with ID {MovieID} not found on TMDB.", movieId);
                    return;
                }

                // Step 3: Save movie details to the database
                await _databaseService.InsertMovieAsync(movie);
                _logger.LogInformation("Movie metadata saved to database for MovieID: {MovieID}", movieId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing metadata for file hash: {FileHash}", fileHash);
                throw;
            }
        }
    }
}
