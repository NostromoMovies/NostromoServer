using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Database;
using Nostromo.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nostromo.Server.Services;
using db = Nostromo.Server.Database;
using FluentNHibernate.Conventions.Inspections;

[ApiController]
[Route("api/[controller]")]
public class WatchListController : ControllerBase
{
    private readonly NostromoDbContext _context;
    private readonly SelectedProfileService _selectedProfileService;

    public WatchListController(NostromoDbContext context, SelectedProfileService selectedProfileService)
    {
        _context = context;
        _selectedProfileService = selectedProfileService;
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
        var profileId = _selectedProfileService.GetSelectedProfileId();
        var watchList = new db.WatchList
        {
            UserID = userId.Value,
            Name = title,
            ProfileID = profileId.Value,
        };

        _context.WatchLists.Add(watchList);
        await _context.SaveChangesAsync();

        return Ok(new { watchList.WatchListID });
    }

    [HttpPost("tv/{watchListId}/add/{tvShowId}")]
    public async Task<IActionResult> AddTvToWatchList(int watchListId, int tvShowId)
    {
        var watchList = await _context.WatchLists.FindAsync(watchListId);
        if (watchList == null)
            return NotFound("Watch list not found.");

        var tvShow = await _context.TvShows.FindAsync(tvShowId);

        if (tvShow == null)
            return NotFound("Tv show not found.");


        var existingItem = await _context.WatchListItems.FirstOrDefaultAsync(tvi => tvi.WatchListID == watchListId && tvi.TvShowID == tvShowId);

        if (existingItem == null)
        {
            var item = new db.WatchListItem
            {
                WatchListID = watchListId,
                TvShowID = tvShowId,
                MovieID = null
            };
            _context.WatchListItems.Add(item);
            await _context.SaveChangesAsync();
        }

        return Ok("Show added to watch list.");
    }

    [HttpPost("{watchListId}/add/{movieId}")]
    public async Task<IActionResult> AddMoviesToWatchList(int watchListId, int movieId)
    {
        var watchList = await _context.WatchLists.FindAsync(watchListId);
        if (watchList == null)
            return NotFound("Watch list not found.");

        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null)
            return NotFound("Movie not found.");

        var existingItem = await _context.WatchListItems
            .FirstOrDefaultAsync(wli => wli.WatchListID == watchListId && wli.MovieID == movieId);

        if (existingItem == null)
        {
            var item = new db.WatchListItem
            {
                WatchListID = watchListId,
                MovieID = movieId,
                TvShowID = null
            };
            _context.WatchListItems.Add(item);
            await _context.SaveChangesAsync();
        }


        return Ok("Movies added to watch list.");
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
    
    [HttpDelete("tv/{watchListId}/remove/{tvShowId}")]
    public async Task<IActionResult> RemoveTvFromWatchList(int watchListId, int tvShowId)
    {
        var watchListItem = await _context.WatchListItems
            .FirstOrDefaultAsync(wli => wli.WatchListID == watchListId && wli.TvShowID == tvShowId);

        if (watchListItem == null)
            return NotFound("This tv show is not in the specified watch list.");

        _context.WatchListItems.Remove(watchListItem);
        await _context.SaveChangesAsync();

        return Ok("Tv show removed from watch list.");
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

        var profileId = _selectedProfileService.GetSelectedProfileId();

        var watchLists = await _context.WatchLists
            .Where(w => w.UserID == userId && w.ProfileID == profileId)
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
    
    [HttpGet("{watchListId}/tv")]
    public async Task<IActionResult> GetAllTvInWatchList(int watchListId)
    {
        var watchListExists = await _context.WatchLists.AnyAsync(w => w.WatchListID == watchListId);
        if (!watchListExists)
            return NotFound("Watch list not found.");

        var tvShows = await _context.WatchListItems
            .Where(wli => wli.WatchListID == watchListId)
            .Include(wli => wli.TvShow)
            .Select(wli => wli.TvShow)
            .ToListAsync();

        return Ok(tvShows);
    }
    [HttpGet("{watchListId}")]
    public async Task<IActionResult> GetWatchListById(int watchListId)
    {
        return Ok(await _context.WatchLists.FindAsync(watchListId));
    }
}