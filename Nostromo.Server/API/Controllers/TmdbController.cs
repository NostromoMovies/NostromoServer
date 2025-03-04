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
    [ProducesResponseType(typeof(SuccessResponse<TmdbMovieResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
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

    [HttpGet("search/keyword")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TmdbMovieResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IResult> SearchMoviesByKeyword([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return ApiResults.BadRequest("Keyword is required");
        }

        var (results, totalResults) = await _tmdbService.SearchMoviesByKeyword(keyword);

        if (totalResults == 0)
        {
            return ApiResults.NotFound($"No movies found for keyword: {keyword}");
        }

        return ApiResults.SuccessCollection(results);
    }

    [HttpGet("movie_cast/{id}")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TmdbCastMember>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IResult> SearchMoviesByKeyword([FromQuery] int id)
    {
    

        var  results = await _tmdbService.GetMovieCreditsAsync(id);

        return ApiResults.Success(results);
    }

    [HttpGet("movie/{id}/recommendations")]
    [ProducesResponseType(typeof(SuccessResponse<TmdbRecommendationsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetMovieRecommendations(int id)
    {
        var recommendations = await _tmdbService.GetRecommendation(id);

        if (recommendations == null || recommendations.Results == null || !recommendations.Results.Any())
        {
            return ApiResults.NotFound($"No recommendations found for movie ID: {id}");
        }

        return ApiResults.Success(recommendations);
    }
}