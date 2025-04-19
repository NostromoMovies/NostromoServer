using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Database;
using Nostromo.Models;
using System.Threading.Tasks;
using db = Nostromo.Server.Database;

[ApiController]
[Route("api/[controller]")]
public class WatchListController : ControllerBase
{
    private readonly NostromoDbContext _context;

    public WatchListController(NostromoDbContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateWatchList([FromBody] db.WatchList list)
    {
        _context.WatchLists.Add(list);
        await _context.SaveChangesAsync();
        return Ok(new { list.WatchListID });
    }

    [HttpPost("{watchListId}/add/{movieId}")]
    public async Task<IActionResult> AddMovieToWatchList(int watchListId, int movieId)
    {
        var watchList = await _context.WatchLists.FindAsync(watchListId);
        if (watchList == null)
            return NotFound("Watch list not found.");

        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null)
            return NotFound("Movie not found.");

        var item = new db.WatchListItem
        {
            WatchListID = watchListId,
            MovieID = movieId
        };

        _context.WatchListItems.Add(item);
        await _context.SaveChangesAsync();

        return Ok("Movie added to watch list.");
    }
}