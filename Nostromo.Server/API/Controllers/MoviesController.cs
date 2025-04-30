using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.API.Models;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Utilities;
using System.Security.Claims;
using Nostromo.Models;
using System.Linq;

namespace Nostromo.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;

    public MoviesController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TMDBMovie>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IResult> GetMovies()
    {
        var movies = await _movieRepository.GetAllAsync();

        return ApiResults.SuccessCollection(movies);
    }

    [HttpGet("{id}/poster")]
    public async Task<IResult> GetPoster(int id)
    {
        var (exists, path) = await _movieRepository.GetPosterPathAsync(id);
        if (!exists)
            return ApiResults.NotFound("Poster not found");
        return ApiResults.PhysicalFile(path, "image/jpeg");
    }

    [HttpGet("id/{tmdbId:int}")]
    [ProducesResponseType(typeof(SuccessResponse<MovieDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetMovieById(int tmdbId)
    {
        var movie = await _movieRepository.GetByTMDBIdAsync(tmdbId);

        if (movie is null)
            return Results.NotFound(new ErrorResponse(
                "1.0",
                new ApiError(404, "Movie not found.")));

        var dto = new MovieDto(
            movie.MovieID,
            movie.Title,
            movie.Overview,
            movie.ReleaseDate,
            movie.Genres.Select(g => g.Name),
            movie.PosterPath,
            movie.BackdropPath,
            movie.Runtime);

        return Results.Ok(new SuccessResponse<MovieDto>("1.0", dto));
    }
}
