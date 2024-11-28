using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Utilities;
using System.Security.Claims;

namespace Nostromo.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;

    public MoviesController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TMDBMovie>>> GetMovies()
    {
        var movies = await _movieRepository.GetAllAsync();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Ok(movies);
    }

    [HttpGet("{id}/poster")]
    public async Task<IActionResult> GetPoster(int id)
    {
        var (exists, path) = await _movieRepository.GetPosterPathAsync(id);
        if (!exists)
            return NotFound();

        return PhysicalFile(path, "image/jpeg");
    }
}
