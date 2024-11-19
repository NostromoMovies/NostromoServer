using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nostromo.Models;
using Nostromo.Server.Services;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TmdbController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<TmdbController> _logger;
        private readonly string _tmdbApiKey;
        private readonly string _tmdbBaseUrl = "https://api.themoviedb.org/3";

        public TmdbController(
            HttpClient httpClient,
            IDatabaseService databaseService,
            IConfiguration configuration,
            ILogger<TmdbController> logger)
        {
            _httpClient = httpClient;
            _databaseService = databaseService;
            _logger = logger;
            _tmdbApiKey = configuration["TMDB:ApiKey"]
                ?? throw new ArgumentNullException("TMDB API key not configured");

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_tmdbBaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<Dictionary<int, string>> GetGenreDictionary()
        {
            try
            {
                var genreUrl = $"/genre/movie/list?api_key={_tmdbApiKey}";
                var genreResponse = await _httpClient.GetFromJsonAsync<GenreResponse>(genreUrl);
                var genreDict = new Dictionary<int, string>();

                if (genreResponse?.genres != null)
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

        private async Task<int?> GetMovieRuntime(int movieId)
        {
            try
            {
                var movieDetailsUrl = $"/movie/{movieId}?api_key={_tmdbApiKey}";
                var movieDetails = await _httpClient.GetFromJsonAsync<TmdbMovie>(movieDetailsUrl);
                return movieDetails?.runtime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching runtime for movie {MovieId}", movieId);
                return null;
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<TmdbResponse>> SearchMovies(string query = "Inception")
        {
            try
            {
                _logger.LogInformation("Searching for movies with query: {Query}", query);

                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { Message = "Search query is required" });
                }

                // Fetch genres first
                var genreDict = await GetGenreDictionary();

                // Search movies
                var tmdbUrl = $"/search/movie?api_key={_tmdbApiKey}&query={Uri.EscapeDataString(query)}";
                var response = await _httpClient.GetFromJsonAsync<TmdbResponse>(tmdbUrl);

                if (response?.results == null || !response.results.Any())
                {
                    _logger.LogInformation("No results found for query: {Query}", query);
                    return Ok(new { Message = "No results found", Results = Array.Empty<TmdbMovie>() });
                }

                // Process each movie
                foreach (var movie in response.results)
                {
                    try
                    {
                        // Fetch runtime
                        movie.runtime = await GetMovieRuntime(movie.id);

                        // Map genre names for logging
                        var genres = movie.genreIds
                            .Select(id => genreDict.ContainsKey(id) ? genreDict[id] : "Unknown")
                            .ToList();

                        _logger.LogDebug(
                            "Processing movie: {Title}, Genres: {Genres}, Runtime: {Runtime} minutes, Release Date: {ReleaseDate}",
                            movie.title,
                            string.Join(", ", genres),
                            movie.runtime,
                            movie.releaseDate);

                        // Save to database
                        await _databaseService.InsertMovieAsync(movie);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing movie {Title} (ID: {Id})", movie.title, movie.id);
                        // Continue processing other movies even if one fails
                    }
                }

                _logger.LogInformation("Successfully processed {Count} movies for query: {Query}",
                    response.results.Count, query);

                return Ok(new
                {
                    Message = $"Found {response.results.Count} results",
                    Results = response.results
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "TMDB API request error for query: {Query}", query);
                return StatusCode(500, new { Message = "Error contacting TMDB API" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing search query: {Query}", query);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        // You might want to add additional endpoints for other TMDB API features
        [HttpGet("movie/{id}")]
        public async Task<ActionResult<TmdbMovie>> GetMovie(int id)
        {
            try
            {
                var movieUrl = $"/movie/{id}?api_key={_tmdbApiKey}";
                var movie = await _httpClient.GetFromJsonAsync<TmdbMovie>(movieUrl);

                if (movie == null)
                {
                    return NotFound(new { Message = $"Movie with ID {id} not found" });
                }

                // Save to database
                await _databaseService.InsertMovieAsync(movie);

                return Ok(movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movie details for ID: {MovieId}", id);
                return StatusCode(500, new { Message = "Error fetching movie details" });
            }
        }
    }
}