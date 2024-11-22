using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nostromo.Server.API.Models;
using Nostromo.Server.Settings;
using System.Net.Http.Json;
using Quartz;

namespace Nostromo.Server.Scheduling.Jobs
{
    public class DownloadTmdbImageJob : DownloadImageBaseJob
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsProvider _settingsProvider;

        public static readonly string movieId;

        public override string Type => "DownloadingImage";
        public override string RemoteURL => "https://image.tmdb.org/t/p/original";

        public DownloadTmdbImageJob(
            ILogger<DownloadTmdbImageJob> logger,
            IHttpClientFactory httpClientFactory,
            ISettingsProvider settingsProvider,
            IHttpContextAccessor httpContextAccessor)
            : base(logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _settingsProvider = settingsProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task ProcessJob()
        {
            try
            {
                var settings = _settingsProvider.GetSettings();
                var baseUrl = $"http://localhost:{settings.ServerPort}/api/tmdb";
                //TODO: var request = _httpContextAccessor.HttpContext.Request;
                var response = await _httpClient.GetFromJsonAsync<TmdbImageCollection>($"{baseUrl}/movie/{ImageId}/images");

                if (response?.Posters == null || !response.Posters.Any())
                {
                    throw new Exception($"No posters found for movie {ImageId}");
                }

                // Get the first poster
                var poster = response.Posters.First();
                RemotePath = poster.FilePath;

                _logger.LogInformation($"Starting download of TMDB poster {ImageId} from path {RemotePath}");
                await base.ProcessJob();
                _logger.LogInformation($"Successfully downloaded TMDB poster {ImageId} to {SavePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process TMDB image job for {ImageId}: {ex.Message}");
                throw;
            }
        }
    }
}
