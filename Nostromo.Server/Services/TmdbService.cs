using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nostromo.Models;
using Nostromo.Server.API.Models;
using Nostromo.Server.Settings;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Nostromo.Server.Services
{

    public interface ITmdbService
    {
        Task<TmdbMovie> GetMovieById(int id);
        Task<TmdbImageCollection> GetMovieImagesById(int id);
        Task<int?> GetMovieRuntime(int movieId);
        Task<(IEnumerable<TmdbMovie> Results, int TotalResults)> SearchMovies(string query);
        Task<Dictionary<int, string>> GetGenreDictionary();
    }

    public class TmdbService : ITmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<TmdbService> _logger;
        private readonly string _tmdbApiKey;

        public TmdbService(
            HttpClient httpClient,
            IDatabaseService databaseService,
            IOptions<ServerSettings> serverSettings,
            ILogger<TmdbService> logger)
        {
            _httpClient = httpClient;
            _databaseService = databaseService;
            _logger = logger;
            _tmdbApiKey = serverSettings.Value.TmdbApiKey
                ?? throw new ArgumentNullException(nameof(serverSettings), "TMDB API key not configured");

            _httpClient.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Dictionary<int, string>> GetGenreDictionary()
        {
            try
            {
                var genreUrl = $"genre/movie/list?api_key={_tmdbApiKey}";
                var genreResponse = await _httpClient.GetFromJsonAsync<GenreResponse>(genreUrl)
                    ?? throw new NotFoundException("Genre list not found");

                var genreDict = new Dictionary<int, string>();

                if (genreResponse.genres != null)
                {
                    foreach (var genre in genreResponse.genres)
                    {
                        genreDict[genre.id] = genre.name;
                        await _databaseService.InsertGenreAsync(genre);
                    }
                    _logger.LogInformation("Successfully fetched and stored {Count} genres", genreResponse.genres.Count);
                }
                return genreDict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching genre dictionary");
                throw;
            }
        }

        public async Task<TmdbMovie> GetMovieById(int id)
        {
            try
            {
                var movieUrl = $"movie/{id}?api_key={_tmdbApiKey}";
                var movie = await _httpClient.GetFromJsonAsync<TmdbMovie>(movieUrl)
                    ?? throw new NotFoundException($"Movie with ID {id} not found");

                await _databaseService.InsertMovieAsync(movie);
                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movie details for ID: {MovieId}", id);
                throw;
            }
        }

        public async Task<TmdbImageCollection> GetMovieImagesById(int id)
        {
            try
            {
                var imageUrl = $"movie/{id}/images?api_key={_tmdbApiKey}";
                var images = await _httpClient.GetFromJsonAsync<TmdbImageCollection>(imageUrl)
                    ?? throw new NotFoundException($"Images for movie with ID {id} not found");
                return images;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching images for movie with ID: {MovieId}", id);
                throw;
            }
        }

        public async Task<int?> GetMovieRuntime(int movieId)
        {
            try
            {
                var movie = await GetMovieById(movieId);
                return movie.runtime;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching runtime for movie {MovieId}", movieId);
                throw;
            }
        }

        public async Task<(IEnumerable<TmdbMovie> Results, int TotalResults)> SearchMovies(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    throw new ArgumentException("Search query cannot be empty", nameof(query));
                }

                var tmdbUrl = $"search/movie?api_key={_tmdbApiKey}&query={Uri.EscapeDataString(query)}";
                var response = await _httpClient.GetFromJsonAsync<TmdbResponse>(tmdbUrl)
                    ?? throw new NotFoundException("No search results found");

                if (response.results == null || !response.results.Any())
                {
                    return (Array.Empty<TmdbMovie>(), 0);
                }

                var genreDict = await GetGenreDictionary();

                // Process each movie
                foreach (var movie in response.results)
                {
                    try
                    {
                        movie.runtime = await GetMovieRuntime(movie.id);
                        await _databaseService.InsertMovieAsync(movie);

                        var genres = movie.genreIds
                            .Select(id => genreDict.ContainsKey(id) ? genreDict[id] : "Unknown")
                            .ToList();

                        _logger.LogDebug(
                            "Processed movie: {Title}, Genres: {Genres}, Runtime: {Runtime}",
                            movie.title,
                            string.Join(", ", genres),
                            movie.runtime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing movie {Title} (ID: {Id})", movie.title, movie.id);
                        // Continue processing other movies
                    }
                }

                return (response.results, response.results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies with query: {Query}", query);
                throw;
            }
        }
    }

    public class NotFoundException(string message) : Exception(message)
    {

    }
}
