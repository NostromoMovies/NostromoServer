using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nostromo.Models;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaDisplayController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

       
        public MediaDisplayController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        
        [HttpGet("searchMedia")]
        public async Task<IActionResult> SearchMedia([FromQuery] string mediaName)
        {
           var movies = new List<TmdbMovie>();
            if (string.IsNullOrWhiteSpace(mediaName))
            {
                return BadRequest("No Results.");
            }


            List<TmdbMovie> movieList = await _databaseService.GetMediaListAsync(mediaName, movies);


            var filteredMedia = movieList

                .Where(movie => movie.title.Contains(mediaName,System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(filteredMedia);
        }
        
    }
}
