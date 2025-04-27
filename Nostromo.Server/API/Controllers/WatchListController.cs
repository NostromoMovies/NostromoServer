using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Database;
using Nostromo.Models;
using System.Linq;
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

    private async Task<int?> GetLoggedInUserIdAsync()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return await _context.AuthTokens
            .Where(t => t.Token == token)
            .Select(t => (int?)t.UserId)
            .FirstOrDefaultAsync();
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateWatchList([FromBody] string title)
    {
        var userId = await GetLoggedInUserIdAsync();
        if (userId == null)
            return Unauthorized("Missing or invalid token.");

        var watchList = new db.WatchList
        {
            UserID = userId.Value,
            Name = title
        };

        _context.WatchLists.Add(watchList);
        await _context.SaveChangesAsync();

        return Ok(new { watchList.WatchListID });
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

    [HttpDelete("{watchListId}/remove/{movieId}")]
    public async Task<IActionResult> RemoveMovieFromWatchList(int watchListId, int movieId)
    {
        var watchListItem = await _context.WatchListItems
            .FirstOrDefaultAsync(wli => wli.WatchListID == watchListId && wli.MovieID == movieId);

        if (watchListItem == null)
            return NotFound("This movie is not in the specified watch list.");

        _context.WatchListItems.Remove(watchListItem);
        await _context.SaveChangesAsync();

        return Ok("Movie removed from watch list.");
    }

    [HttpDelete("{watchListId}/delete")]
    public async Task<IActionResult> DeleteWatchList(int watchListId)
    {
        var watchList = await _context.WatchLists.FindAsync(watchListId);
        if (watchList == null)
            return NotFound("Watch list not found.");

        _context.WatchLists.Remove(watchList);
        await _context.SaveChangesAsync();

        return Ok("Watch list deleted.");
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllWatchLists()
    {
        var userId = await GetLoggedInUserIdAsync();
        if (userId == null)
            return Unauthorized("Missing or invalid token.");

        var watchLists = await _context.WatchLists
            .Where(w => w.UserID == userId)
            .ToListAsync();

        return Ok(watchLists);
    }

    [HttpGet("{watchListId}/movies")]
    public async Task<IActionResult> GetAllMoviesInWatchList(int watchListId)
    {
        var watchListExists = await _context.WatchLists.AnyAsync(w => w.WatchListID == watchListId);
        if (!watchListExists)
            return NotFound("Watch list not found.");

        var movies = await _context.WatchListItems
            .Where(wli => wli.WatchListID == watchListId)
            .Include(wli => wli.Movie)
            .Select(wli => wli.Movie)
            .ToListAsync();

        return Ok(movies);
    }
    [HttpGet("{watchListId}")]
    public async Task<IActionResult> GetWatchListById(int watchListId)
    {
        return Ok(await _context.WatchLists.FindAsync(watchListId));
    }
}