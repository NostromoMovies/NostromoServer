using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Database;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly NostromoDbContext _context;

    public MoviesController(NostromoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TMDBMovie>>> GetMovies()
    {
        return await _context.Movies.ToListAsync();
    }

    [HttpGet("{id}/poster")]
    public async Task<IActionResult> GetPoster(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();

        var imagePath = Path.Combine(Utils.ApplicationPath, $"posters/{id}_poster.jpg");

        if (!System.IO.File.Exists(imagePath))
            return NotFound();

        return PhysicalFile(imagePath, "image/jpeg"); // context
    }
}
