using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nostromo.Server.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Scheduling.Jobs
{
    public class DownloadTmdbImageJob : DownloadImageBaseJob
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public override string Type => "DownloadingImage";
        public override string RemoteURL => "https://image.tmdb.org/t/p/original";

        public DownloadTmdbImageJob(
            ILogger<DownloadTmdbImageJob> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
            : base(logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5000/api/tmdb";
        }

        public override async Task ProcessJob()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<TmdbImageCollection>($"{_apiBaseUrl}/movie/{ImageId}/images");

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
