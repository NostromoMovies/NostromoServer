   using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Nostromo.Server.Services;
using Nostromo.Server.API.Models;
using Nostromo.Server.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreditsController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;

        public CreditsController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpGet("cast/{mediaType}/{mediaId}")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TmdbCastMember>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> GetCastByMediaId(int mediaId, string mediaType)
        {
            if (mediaId <= 0)
            {
                return ApiResults.BadRequest("Invalid movie ID.");
            }

            var cast = await _databaseService.GetCastByMediaIdAsync(mediaId, mediaType);

            if (cast == null || cast.Count == 0)
            {
                return ApiResults.NotFound($"No cast found for {mediaType} ID {mediaId}.");
            }

            return ApiResults.SuccessCollection(cast);
        }

        [HttpGet("crew/{mediaType}/{mediaId}")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TmdbCrewMember>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> GetCrewByMovieId(int mediaId, string mediaType)
        {
            if (mediaId <= 0)
            {
                return ApiResults.BadRequest($"Invalid {mediaType} ID.");
            }

            var crew = await _databaseService.GetCrewByMediaIdAsync(mediaId, mediaType);

            if (crew == null || crew.Count == 0)
            {
                return ApiResults.NotFound($"No crew found for {mediaType} ID {mediaId}.");
            }

            return ApiResults.SuccessCollection(crew);
        }
    }
}
