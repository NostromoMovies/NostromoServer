using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.API.Models;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Utilities;
using System.Security.Claims;

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
}
