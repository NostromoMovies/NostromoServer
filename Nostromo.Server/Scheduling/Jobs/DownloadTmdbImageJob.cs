using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nostromo.Server.API.Models;
using Nostromo.Server.Settings;
using System.Net.Http.Json;  // Add this for GetFromJsonAsync
using Nostromo.Server.Utilities;
using System.Text.Json;
using System.IO;
using Nostromo.Server.API.Enums;

namespace Nostromo.Server.Scheduling.Jobs
{
    public class DownloadTmdbImageJob : DownloadImageBaseJob
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsProvider _settingsProvider;
        public const string MEDIA_ID_KEY = "MediaId";
        public const string MEDIA_TYPE_KEY = "MediaType";
        public const string SEASON_NUMBER_KEY = "SeasonNumber";
        public const string EPISODES_NUMBER_KEY = "EpisodeNumber";
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
                _logger.LogInformation($"Downloading TMDB image from {RemoteURL}");
                ImageId = Context.JobDetail.JobDataMap.GetInt(MEDIA_ID_KEY);
                string mediaTypeString = Context.JobDetail.JobDataMap.GetString(MEDIA_TYPE_KEY); 
                MediaTypes mediaType = (MediaTypes)Enum.Parse(typeof(MediaTypes), mediaTypeString, true);
                
                int? seasonNumber = Context.JobDetail.JobDataMap.ContainsKey(SEASON_NUMBER_KEY) ? (int?)Context.JobDetail.JobDataMap.GetInt(SEASON_NUMBER_KEY): null;
                int? episodeNumber = Context.JobDetail.JobDataMap.ContainsKey(EPISODES_NUMBER_KEY) ? (int?)Context.JobDetail.JobDataMap.GetInt(EPISODES_NUMBER_KEY) : null;

                var settings = _settingsProvider.GetSettings();
                var baseUrl = $"http://localhost:{settings.ServerPort}/api/tmdb";

                string Url = $"{baseUrl}/media/{mediaType}/{ImageId}/images";

                var queryParams = new List<string>();

                if (seasonNumber.HasValue)
                {
                    queryParams.Add($"seasonNumber={seasonNumber}");
                }

                if (episodeNumber.HasValue)
                {
                    queryParams.Add($"episodeNumber={episodeNumber}");
                }

                if (queryParams.Count > 0)
                {
                    Url = Url + "?" + string.Join("&", queryParams);
                }
                
                var response = await _httpClient.GetFromJsonAsync<TmdbImageWrapper>(Url); 

                var actualImages = response?.Data;
                _logger.LogInformation($"Downloading TMDB image from {actualImages}");

                var poster = new TmdbImage();
                if (mediaType == MediaTypes.Episode)
                {
                    if (actualImages?.Stills == null || !actualImages.Stills.Any())
                    {
                        throw new Exception($"No posters found for {ImageId}_Season_{seasonNumber}_Episode_{episodeNumber}");
                    }
                    poster = actualImages.Stills.First();
                }
                else
                {
                    if (actualImages?.Posters == null || !actualImages.Posters.Any())
                    {
                        throw new Exception($"No posters found for {mediaType} {ImageId}");
                    }
                    poster = actualImages.Posters.First();
                }
                

                
                RemotePath = poster.FilePath;

                Console.WriteLine($"Printing Path: {RemotePath}");

                var posterDirectory = Path.Combine(Utils.ApplicationPath, "posters");
                Directory.CreateDirectory(posterDirectory);

                string fileName = mediaType switch
                {
                    MediaTypes.Movie => $"{ImageId}_poster.jpg",
                    MediaTypes.Tv => $"{ImageId}_poster.jpg",
                    MediaTypes.Season => $"{ImageId}_season_{seasonNumber}_poster.jpg",
                    MediaTypes.Episode => $"{ImageId}_season_{seasonNumber}_episode_{episodeNumber}_poster.jpg",
                };
                
                SavePath = Path.Combine(posterDirectory, fileName);
                _logger.LogInformation($"Starting download of {mediaType} poster {ImageId} from path {RemotePath}");

                var posterBytes = await _httpClient.GetByteArrayAsync($"{RemoteURL}/{RemotePath.TrimStart('/')}");
                await File.WriteAllBytesAsync(SavePath, posterBytes);

                _logger.LogInformation($"Successfully downloaded {mediaType} poster {ImageId} to {SavePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process TMDB image job for {ImageId}: {ex.Message}");
                throw;
            }
        }
    }
}