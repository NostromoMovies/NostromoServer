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
        var movie = await _tmdbService.GetMovieById(id);
        return ApiResults.Success(movie);
    }

    [HttpGet("movie/{id}/images")]
    public async Task<IResult> GetMovieImagesById(int id)
    {
        var images = await _tmdbService.GetMovieImagesById(id);
        return ApiResults.Success(images);
    }

    [HttpGet("movie_runtime/{id}")]
    public async Task<IResult> GetMovieRuntime(int id)
    {
        var runtime = await _tmdbService.GetMovieRuntime(id);
        return ApiResults.Success(runtime);
    }

    [HttpGet("search")]
    public async Task<IResult> SearchMovies([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return ApiResults.BadRequest("Search query is required");
        }

        var (results, totalResults) = await _tmdbService.SearchMovies(query);
        return ApiResults.SuccessCollection(results);
    }
}