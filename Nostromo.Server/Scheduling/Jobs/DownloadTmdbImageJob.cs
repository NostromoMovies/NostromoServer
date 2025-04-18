﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nostromo.Server.API.Models;
using Nostromo.Server.Settings;
using System.Net.Http.Json;  // Add this for GetFromJsonAsync
using Nostromo.Server.Utilities;
using System.Text.Json;
using System.IO;

namespace Nostromo.Server.Scheduling.Jobs
{
    public class DownloadTmdbImageJob : DownloadImageBaseJob
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsProvider _settingsProvider;
        public const string MOVIE_ID_KEY = "MovieId";
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
                ImageId = Context.JobDetail.JobDataMap.GetInt(MOVIE_ID_KEY);

                var settings = _settingsProvider.GetSettings();
                var baseUrl = $"http://localhost:{settings.ServerPort}/api/tmdb";

                var response = await _httpClient.GetFromJsonAsync<TmdbImageWrapper>($"{baseUrl}/movie/{ImageId}/images");

                var actualImages = response?.Data;

                if (actualImages?.Posters == null || !actualImages.Posters.Any())
                {
                    throw new Exception($"No posters found for movie {ImageId}");
                }

                var poster = actualImages.Posters.First();
                RemotePath = poster.FilePath;

                Console.WriteLine($"Printing Path: {RemotePath}");

                var posterDirectory = Path.Combine(Utils.ApplicationPath, "posters");
                Directory.CreateDirectory(posterDirectory);

                SavePath = Path.Combine(posterDirectory, $"{ImageId}_poster.jpg");
                _logger.LogInformation($"Starting download of TMDB poster {ImageId} from path {RemotePath}");

                var posterBytes = await _httpClient.GetByteArrayAsync($"{RemoteURL}/{RemotePath.TrimStart('/')}");
                await File.WriteAllBytesAsync(SavePath, posterBytes);

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