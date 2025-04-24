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

        [HttpGet("cast/{movieId}")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TmdbCastMember>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> GetCastByMovieId(int movieId)
        {
            if (movieId <= 0)
            {
                return ApiResults.BadRequest("Invalid movie ID.");
            }

            var cast = await _databaseService.GetCastByMovieIdAsync(movieId);

            if (cast == null || cast.Count == 0)
            {
                return ApiResults.NotFound($"No cast found for movie ID {movieId}.");
            }

            return ApiResults.SuccessCollection(cast);
        }

        [HttpGet("crew/{movieId}")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TmdbCrewMember>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> GetCrewByMovieId(int movieId)
        {
            if (movieId <= 0)
            {
                return ApiResults.BadRequest("Invalid movie ID.");
            }

            var crew = await _databaseService.GetCrewByMovieIdAsync(movieId);

            if (crew == null || crew.Count == 0)
            {
                return ApiResults.NotFound($"No crew found for movie ID {movieId}.");
            }

            return ApiResults.SuccessCollection(crew);
        }
        
        [HttpGet("tv/cast/{showId}")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TmdbCastMember>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> GetCastByTvId(int showId)
        {
            if (showId <= 0)
            {
                return ApiResults.BadRequest("Invalid show ID.");
            }

            var cast = await _databaseService.GetCastByTvMediaIdAsync(showId);

            if (cast == null || cast.Count == 0)
            {
                return ApiResults.NotFound($"No cast found for show ID {showId}.");
            }

            return ApiResults.SuccessCollection(cast);
        }

        [HttpGet("tv/crew/{showId}")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TmdbCrewMember>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> GetCrewByTvId(int showId)
        { 
            if (showId <= 0)
            {
                return ApiResults.BadRequest("Invalid show ID.");
            }

            var crew = await _databaseService.GetCrewByTvMediaIdAsync(showId);

            if (crew == null || crew.Count == 0)
            {
                return ApiResults.NotFound($"No crew found for show ID {showId}.");
            }

            return ApiResults.SuccessCollection(crew);
        }
        
        [HttpGet("episode/cast/{episodeId}")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TmdbCastMember>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> GetCastByEpisodeId(int episodeId)
        {
            if (episodeId <= 0)
            {
                return ApiResults.BadRequest("Invalid episode ID.");
            }

            var cast = await _databaseService.GetCastByTvMediaIdAsync(episodeId);

            if (cast == null || cast.Count == 0)
            {
                return ApiResults.NotFound($"No cast found for episode ID {episodeId}.");
            }

            return ApiResults.SuccessCollection(cast);
        }

        [HttpGet("episode/crew/{episodeId}")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TmdbCrewMember>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> GetCrewByEpisodeId(int episodeId)
        {
            if (episodeId <= 0)
            {
                return ApiResults.BadRequest("Invalid episode ID.");
            }

            var crew = await _databaseService.GetCrewByTvMediaIdAsync(episodeId);

            if (crew == null || crew.Count == 0)
            {
                return ApiResults.NotFound($"No crew found for episode ID {episodeId}.");
            }

            return ApiResults.SuccessCollection(crew);
        }
    }
}