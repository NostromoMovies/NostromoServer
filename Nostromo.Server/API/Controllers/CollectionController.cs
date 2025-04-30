using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Database;
using Nostromo.Server.Services;
using Nostromo.Server.API.Models;

[ApiController]
[Route("api/[controller]")]
public class  CollectionController : ControllerBase
{
    private readonly IDatabaseService _databaseService;
    private readonly SelectedProfileService _selectedProfileService;
    private readonly NostromoDbContext _context;

    public CollectionController(IDatabaseService databaseService, SelectedProfileService selectedProfileService, NostromoDbContext context)
    {
        _databaseService = databaseService;
        _selectedProfileService = selectedProfileService;
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
    
    [HttpGet("all")]
    public async Task<IActionResult> GetAllCollections()
    {
        try
        {
            var collections = await _databaseService.GetAllCollectionsAsync();
            return Ok(collections);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");
        
        var userId = await GetLoggedInUserIdAsync();
        var profileId = _selectedProfileService.GetSelectedProfileId();
        
        var collection = await _databaseService.CreateCollectionAsync(request.Name, userId, profileId);
        return Ok(collection);
    }

    [HttpPost("{collectionId}/add-items")]
    public async Task<IActionResult> AddToCollection(
        int collectionId,
        [FromQuery(Name = "movieIds")] List<int> movieIds = null,
        [FromQuery(Name = "tvIds")] List<int> tvIds = null)
    {
        if ((movieIds == null || !movieIds.Any()) && (tvIds == null || !tvIds.Any()))
            return BadRequest("At least one movieId or tvId must be provided.");

        try
        {
            await _databaseService.AddItemsToCollectionAsync(
                collectionId,
                movieIds ?? new List<int>(),
                tvIds ?? new List<int>()
            );

            await _databaseService.UpdateCollectionPosterAsync(collectionId);

            return Ok("Items added to collection.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/poster")]
    public async Task<IActionResult> GetCollectionPoster(int id)
    {
        try
        {
            var posterPath = await _databaseService.GetCollectionPosterPathAsync(id);

            if (string.IsNullOrEmpty(posterPath))
            {
                return NotFound();
            }

            if (posterPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(posterPath);
            }

            if (posterPath.StartsWith("/"))
            {
                return Redirect($"https://image.tmdb.org/t/p/original{posterPath}");
            }

            var fullPath = Path.Combine("path_to_your_posters_folder", posterPath);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, "image/jpeg");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCollection(int id)
    {
        try
        {
            var items = await _databaseService.GetCollectionItemsAsync(id);

            if (items == null)
                return NotFound("Collection not found or empty.");

            return Ok(new { items });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{collectionId}/remove-item/{mediaId}/{mediaType}")]
    public async Task<IActionResult> RemoveFromCollection(int collectionId, int mediaId, string mediaType)
    {
        try
        {
            if (mediaType == "movie" || mediaType == "tv")
            {
                await _databaseService.RemoveItemFromCollectionAsync(collectionId, mediaId, mediaType);
                await _databaseService.UpdateCollectionPosterAsync(collectionId);
            }
            else
            {
                return BadRequest("Invalid media type.");
            }

            return Ok("Item removed from collection.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}