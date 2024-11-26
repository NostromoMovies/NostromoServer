using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nostromo.Models;
using Nostromo.Server.API.Models;
using Nostromo.Server.Services;
using Nostromo.Server.Settings;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TmdbController : Controller
    {
        private readonly ITmdbService _tmdbService;
        private readonly ILogger<TmdbController> _logger;

        public TmdbController(ITmdbService tmdbService, ILogger<TmdbController> logger)
        {
            _tmdbService = tmdbService;
            _logger = logger;
        }

        [HttpGet("movie/{id}")]
        public async Task<ActionResult<TmdbMovie>> GetMovieById(int id)
        {
            try
            {
                var movie = await _tmdbService.GetMovieById(id);
                return Ok(movie);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie {Id}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        [HttpGet("movie/{id}/images")]
        public async Task<ActionResult<TmdbImageCollection>> GetMovieImagesById(int id)
        {
            try
            {
                var images = await _tmdbService.GetMovieImagesById(id);
                return Ok(images);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movie images for ID: {MovieId}", id);
                return StatusCode(500, new { Message = "Error fetching movie images" });
            }
        }

        [HttpGet("movie_runtime/{id}")]
        public async Task<ActionResult<int?>> GetMovieRuntime(int id)
        {
            try
            {
                var runtime = await _tmdbService.GetMovieRuntime(id);
                return Ok(runtime);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching runtime for movie {MovieId}", id);
                return StatusCode(500, new { Message = "Error fetching movie runtime" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TmdbMovie>>> SearchMovies([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { Message = "Search query is required" });
                }

                var (results, totalResults) = await _tmdbService.SearchMovies(query);

                return Ok(new
                {
                    Message = $"Found {totalResults} results",
                    Results = results
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return Ok(new { Message = ex.Message, Results = Array.Empty<TmdbMovie>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies with query: {Query}", query);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }
    }
}
