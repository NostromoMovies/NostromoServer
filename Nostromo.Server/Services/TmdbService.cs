using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nostromo.Models;
using Nostromo.Server.API.Models;
using Nostromo.Server.Settings;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using System.Net;

namespace Nostromo.Server.Services
{

    public interface ITmdbService
    {
        Task<TmdbMovieResponse> GetMovieById(int id);
        Task<TmdbImageCollection> GetMovieImagesById(int id);
        Task<int?> GetMovieRuntime(int movieId);
        Task<(IEnumerable<TmdbMovieResponse> Results, int TotalResults)> SearchMovies(string query);
        Task<Dictionary<int, string>> GetGenreDictionary();
        Task<(IEnumerable<TmdbMovieResponse> Results, int TotalResults)> GetRecommendation(string query);
    }

    public class TmdbService : ITmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly IDatabaseService _databaseService; //TODO: remove
        private readonly IMovieRepository _movieRepository;
        private readonly ILogger<TmdbService> _logger;
        private readonly string _tmdbApiKey;
        private readonly IOptions<TmdbSettings> _settings;

        public TmdbService(
            HttpClient httpClient,
            IDatabaseService databaseService,
            IOptions<ServerSettings> serverSettings,
            ILogger<TmdbService> logger,
            IMovieRepository movieRepository,
            IOptions<TmdbSettings> settings)
        {
            _httpClient = httpClient;
            _databaseService = databaseService;
            _movieRepository = movieRepository;
            _logger = logger;
            _tmdbApiKey = serverSettings.Value.TmdbApiKey
                ?? throw new ArgumentNullException(nameof(serverSettings), "TMDB API key not configured");

            _httpClient.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _settings = settings;
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

        public async Task<TmdbMovieResponse> GetMovieById(int id)
        {
            try
            {
                var movieUrl = $"movie/{id}?api_key={_tmdbApiKey}";
                var movie = await _httpClient.GetFromJsonAsync<TmdbMovieResponse>(movieUrl)
                    ?? throw new NotFoundException($"Movie with ID {id} not found");

                var newMovie = new TMDBMovie(movie);

                await _movieRepository.AddAsync(newMovie);

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
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException($"Movie with ID {id} not found");
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

            // weird
        public async Task<(IEnumerable<TmdbMovieResponse> Results, int TotalResults)> SearchMovies(string query)
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
                    return (Array.Empty<TmdbMovieResponse>(), 0);
                }

                var genreDict = await GetGenreDictionary();

                             // apparently not thread-safe
                // Process each movie
                //foreach (var movieResponse in response.results)
                //{
                //    try
                //    {                                     // why?
                //        movieResponse.runtime = await GetMovieRuntime(movieResponse.id);
                //        TMDBMovie movie = new TMDBMovie(movieResponse);
                //        await _movieRepository.AddAsync(movie);

                //        var genres = movieResponse.genreIds
                //            .Select(id => genreDict.ContainsKey(id) ? genreDict[id] : "Unknown")
                //            .ToList();

                //        _logger.LogDebug(
                //            "Processed movie: {Title}, Genres: {Genres}, Runtime: {Runtime}",
                //            movieResponse.title,
                //            string.Join(", ", genres),
                //            movieResponse.runtime);
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogError(ex, "Error processing movie {Title} (ID: {Id})", movieResponse.title, movieResponse.id);
                //        // Continue processing other movies
                //    }
                //}

                return (response.results, response.results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies with query: {Query}", query);
                throw;
            }
        }

        public async Task<(IEnumerable<TmdbMovieResponse> Results, int TotalResults)> GetRecommendation(string query)
        {
            try
            {
                string tmdbUrl;
        
                if (!string.IsNullOrWhiteSpace(query))
                {
                    // Search for movies based on user query
                    tmdbUrl = $"search/movie?api_key={_tmdbApiKey}&query={Uri.EscapeDataString(query)}";
                }
                else
                {
                    // Discover random popular movies
                    var randomPage = new Random().Next(1, 10); 
                    tmdbUrl = $"discover/movie?api_key={_tmdbApiKey}&sort_by=popularity.desc&page={randomPage}";
                }

                var response = await _httpClient.GetFromJsonAsync<TmdbResponse>(tmdbUrl)
                               ?? throw new NotFoundException("No search results found");

                if (response.results == null || !response.results.Any())
                {
                    return (Array.Empty<TmdbMovieResponse>(), 0);
                }

               

                return (response.results, response.results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movies with query: {Query}", query);
                throw;
            }
        }
    }

    public class NotFoundException(string message) : Exception(message)
    {

    }
}
