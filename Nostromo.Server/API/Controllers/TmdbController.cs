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

    [HttpGet("getMoviesReccomendation")]
    public async Task<IResult> GetMoviesReccomendation(string query )
    {
        var results = await _tmdbService.GetRecommendation(query);
        return ApiResults.Success(results);
        
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

    [HttpGet("movie/{id}/credits")]
    [ProducesResponseType(typeof(SuccessResponse<TmdbCastWrapper>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetMovieCast(int id)
    {
        var cast = await _tmdbService.GetMovieCastAsync(id);

        if (cast == null)
        {
            return ApiResults.NotFound($"No cast found for movie ID: {id}");
        }

        return ApiResults.Success(cast);
    }

    [HttpGet("movie/{id}/crew")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TmdbCrewMember>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetMovieCrew(int id)
    {
        var crew = await _tmdbService.GetMovieCrewAsync(id);

        if (crew == null || crew.Count == 0)
        {
            return ApiResults.NotFound($"No crew found for movie ID: {id}");
        }

        return ApiResults.SuccessCollection(crew);
    }
}