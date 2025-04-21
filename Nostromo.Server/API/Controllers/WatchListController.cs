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

    [HttpDelete("{watchListId}/remove/{movieId}")]
    public async Task<IActionResult> RemoveMovieFromWatchList(int watchListId, int movieId)
    {
        var watchList = await _context.WatchLists.FindAsync(watchListId);
        if (watchList == null)
            return NotFound("Watch list not found.");
        
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null)
            return NotFound("Movie not found.");

        
        var watchListItem = await _context.WatchListItems
            .FirstOrDefaultAsync(wli => wli.WatchListID == watchListId && wli.MovieID == movieId);
        
        if (watchListItem == null)
            return NotFound("This movie is not in the specified watch list.");

        _context.WatchListItems.Remove(watchListItem);
        await _context.SaveChangesAsync();

        return Ok("Movie removed from watch list.");
    }

    [HttpDelete("{watchListId}/deleteWatchList")]
    public async Task<IActionResult> DeleteWatchList(int watchListId)
    {
        var watchList = await _context.WatchLists.FindAsync(watchListId);
        if (watchList == null)
            return NotFound("Watch list not found.");
        _context.WatchLists.Remove(watchList);
        await _context.SaveChangesAsync();

        return Ok("Watch list deleted.");

    }

    [HttpGet("{userId}/getAllWatchList")]
    public async Task<IActionResult> GetAllUserWatchList(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if(user == null)
            return NotFound("User not found.");
        var watchLists = await _context.WatchLists.FindAsync(userId);
        return Ok(watchLists);
    }

    [HttpGet("{userId}/getAllMovieInWatchList")]
    public async Task<IActionResult> GetAllMovieInWatchList(int watchListId)
    {
        var watchList = await _context.WatchLists.FindAsync(watchListId);
        if (watchList == null)
            return NotFound("Watch list not found.");
        var movies = await _context.Movies.FindAsync(watchList.WatchListID);
        return Ok(movies);
    }

    [HttpGet("/createWatchList")]
    public async Task<IActionResult> CreateWatchList(int userId,string title)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("User not found.");
    
        // Create new watchlist
        var watchList = new WatchList
        {
            UserID  = userId,
            Name = title, 

        };
    
        // Add to database
        _context.WatchLists.Add(watchList);
    
        try
        {
            await _context.SaveChangesAsync();
            return Ok(watchList); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating watchlist: {ex.Message}");
        }
        
    }

    
}