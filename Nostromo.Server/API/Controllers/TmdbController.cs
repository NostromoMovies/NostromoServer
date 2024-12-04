using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nostromo.Models;
using Nostromo.Server.API.Models;
using Nostromo.Server.Services;

namespace Nostromo.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TmdbController : ControllerBase
{
    private readonly ITmdbService _tmdbService;
    private readonly ILogger<TmdbController> _logger;

    public TmdbController(ITmdbService tmdbService, ILogger<TmdbController> logger)
    {
        _tmdbService = tmdbService;
        _logger = logger;
    }

    [HttpGet("movie/{id}")]
    public async Task<IResult> GetMovieById(int id)
    {
        try
        {
            var movie = await _tmdbService.GetMovieById(id);
            return ApiResults.Success(movie);
        }
        catch (NotFoundException ex)
        {
            return ApiResults.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movie {Id}", id);
            return ApiResults.ServerError("An error occurred");
        }
    }

    [HttpGet("movie/{id}/images")]
    public async Task<IResult> GetMovieImagesById(int id)
    {
        try
        {
            var images = await _tmdbService.GetMovieImagesById(id);
            return ApiResults.Success(images);
        }
        catch (NotFoundException ex)
        {
            return ApiResults.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching movie images for ID: {MovieId}", id);
            return ApiResults.ServerError("Error fetching movie images");
        }
    }

    [HttpGet("movie_runtime/{id}")]
    public async Task<IResult> GetMovieRuntime(int id)
    {
        try
        {
            var runtime = await _tmdbService.GetMovieRuntime(id);
            return ApiResults.Success(runtime);
        }
        catch (NotFoundException ex)
        {
            return ApiResults.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching runtime for movie {MovieId}", id);
            return ApiResults.ServerError("Error fetching movie runtime");
        }
    }

    [HttpGet("search")]
    public async Task<IResult> SearchMovies([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return ApiResults.BadRequest("Search query is required");
            }

            var (results, totalResults) = await _tmdbService.SearchMovies(query);
            return ApiResults.Success(new SearchResult
            {
                TotalResults = totalResults,
                Results = results
            });
        }
        catch (ArgumentException ex)
        {
            return ApiResults.BadRequest(ex.Message);
        }
        catch (NotFoundException)
        {
            return ApiResults.Success(new SearchResult
            {
                TotalResults = 0,
                Results = Array.Empty<TmdbMovieResponse>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching movies with query: {Query}", query);
            return ApiResults.ServerError("An unexpected error occurred");
        }
    }
}

public class SearchResult
{
    public int TotalResults { get; set; }
    public IEnumerable<TmdbMovieResponse> Results { get; set; }
}