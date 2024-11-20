using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nostromo.Models;
using Nostromo.Server.Services;
using Nostromo.Server.Database;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaDisplayController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<MediaDisplayController> _logger;

        public MediaDisplayController(
            IDatabaseService databaseService,
            ILogger<MediaDisplayController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }
        

        [HttpGet("searchMedia")]
        public async Task<ActionResult<IEnumerable<TmdbMovie>>> SearchMedia([FromQuery] string mediaName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mediaName))
                {
                    return BadRequest(new { Message = "Search term is required" });
                }

                _logger.LogInformation("Searching for media with name: {MediaName}", mediaName);

                var movies = await _databaseService.SearchMoviesAsync(mediaName);

                if (!movies.Any())
                {
                    return Ok(new
                    {
                        Message = "No movies found matching the search criteria",
                        Results = Array.Empty<TmdbMovie>()
                    });
                }

                // Convert database entities to API model
                var results = movies.Select(movie => new TmdbMovie
                {
                    id = movie.MovieID,
                    title = movie.Title,
                    originalTitle = movie.OriginalTitle,
                    overview = movie.Overview,
                    posterPath = movie.PosterPath,
                    backdropPath = movie.BackdropPath,
                    releaseDate = movie.ReleaseDate,
                    adult = movie.Adult,
                    popularity = Convert.ToSingle(movie.Popularity),
                    voteCount = movie.VoteCount,
                    voteAverage = Convert.ToSingle(movie.VoteAverage),
                    runtime = movie.Runtime,
                    genreIds = movie.Genres?.Select(g => g.GenreID).ToList() ?? new List<int>()
                }).ToList();

                _logger.LogInformation("Found {Count} movies matching search term: {MediaName}",
                    results.Count, mediaName);

                return Ok(new
                {
                    Message = $"Found {results.Count} results",
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for media with name: {MediaName}", mediaName);
                return StatusCode(500, new { Message = "An error occurred while searching for media" });
            }
        }

        // You might want to add other endpoints for getting movie details, genres, etc.
        [HttpGet("{id}")]
        public async Task<ActionResult<TmdbMovie>> GetMovie(int id)
        {
            try
            {
                var movie = await _databaseService.GetMovieAsync(id);

                if (movie == null)
                {
                    return NotFound(new { Message = $"Movie with ID {id} not found" });
                }

                var result = new TmdbMovie
                {
                    id = movie.MovieID,
                    title = movie.Title,
                    originalTitle = movie.OriginalTitle,
                    overview = movie.Overview,
                    posterPath = movie.PosterPath,
                    backdropPath = movie.BackdropPath,
                    releaseDate = movie.ReleaseDate,
                    adult = movie.Adult,
                    popularity = Convert.ToSingle(movie.Popularity),
                    voteCount = movie.VoteCount,
                    voteAverage = Convert.ToSingle(movie.VoteAverage),
                    runtime = movie.Runtime,
                    genreIds = movie.Genres?.Select(g => g.GenreID).ToList() ?? new List<int>()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movie with ID: {MovieId}", id);
                return StatusCode(500, new { Message = "An error occurred while retrieving the movie" });
            }
        }
        private async Task SendImageToAPI(string imageFilePath)
        {
            try
            {
                using (var client = new HttpClient())
                using (var form = new MultipartFormDataContent())
                {
                    byte[] imageBytes = System.IO.File.ReadAllBytes(imageFilePath);
                    var fileContent = new ByteArrayContent(imageBytes);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                    form.Add(fileContent, "file", Path.GetFileName(imageFilePath));

                    // Replace with the actual URL of the API where you want to send the image
                    string apiUrl = "https://example.com/api/uploadImage";
                    HttpResponseMessage response = await client.PostAsync(apiUrl, form);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"Failed to send image to API. Status: {response.StatusCode}");
                    }
                    else
                    {
                        _logger.LogInformation("Image successfully sent to the API.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending image to external API");
            }
        }
        public async Task<ActionResult<TmdbMovie>> GetAllMovie(int id)
        {
            try
            {
                var results = await _databaseService.getUserhMoviesAsyn(id);

                if results == null)
                {
                    return NotFound(new { Message = $"User colleection not found" });
                }

             

                return Ok(results);
            }
            catch (Exception ex)
            {
                _
                return StatusCode(500, new { Message = "An error occurred while retrieving the movie" });
            }
        }

    }
}