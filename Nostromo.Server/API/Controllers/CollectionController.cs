using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nostromo.Server.Database;
using Nostromo.Server.Services;

[ApiController]
[Route("api/[controller]")]
public class CollectionController : ControllerBase
{
    private readonly IDatabaseService _databaseService;

    public CollectionController(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCollection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Name is required.");

        var collection = await _databaseService.CreateCollectionAsync(name);
        return Ok(collection);
    }

    [HttpPost("{collectionId}/add-items")]
    public async Task<IActionResult> AddToCollection(
        int collectionId,
        [FromQuery(Name = "movieIds")] List<int> movieIds,
        [FromQuery(Name = "tvIds")] List<int> tvIds)
    {
        try
        {
            await _databaseService.AddItemsToCollectionAsync(collectionId, movieIds, tvIds);
            return Ok("Items added to collection.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}