using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.API.Models;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Utilities;
using System.Security.Claims;

namespace Nostromo.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TvShowController : ControllerBase
{
    private readonly ITvShowRepository _tvShowRepository;
    private readonly ITvEpisodeRepository _tvEpisodeRepository;
    private readonly ISeasonRepository _seasonRepository;
    private readonly ITvRecommendationRepository _tvRecommendationRepository;

    public TvShowController(ITvShowRepository tvShowRepository, ITvEpisodeRepository tvEpisodeRepository, ISeasonRepository seasonRepository, ITvRecommendationRepository tvRecommendationRepository)
    {
        _tvShowRepository = tvShowRepository;
        _tvEpisodeRepository = tvEpisodeRepository;
        _seasonRepository = seasonRepository;
        _tvRecommendationRepository = tvRecommendationRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TvShow>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IResult> GetTvShows()
    {
        var shows = await _tvShowRepository.GetAllAsync();

        return ApiResults.SuccessCollection(shows);
    }
    
    [HttpGet("season/{showId}")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<Season>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IResult> GetSeasonsByShowId(int showId)
    {
        var seasons = await _seasonRepository.GetSeasonByShowIdAsync(showId);

        return ApiResults.SuccessCollection(seasons);
    }

    [HttpGet("episode/{seasonId}")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<Episode>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IResult> GetEpisodesByShowId(int seasonId)
    {
        var episodes = await _tvEpisodeRepository.GetEpisodeBySeasonIdAsync(seasonId);

        return ApiResults.SuccessCollection(episodes);
    }

    [HttpGet("{id}/poster")]
    public async Task<IResult> GetPoster(int id)
    {
        var (exists, path) = await _tvShowRepository.GetPosterPathAsync(id);
        if (!exists)
            return ApiResults.NotFound("Poster not found");
        return ApiResults.PhysicalFile(path, "image/jpeg");
    }
    
    [HttpGet("{id}/season/{seasonNumber}/poster")]
    public async Task<IResult> GetSeasonPoster(int id, int seasonNumber)
    {
        var (exists, path) = await _seasonRepository.GetPosterPathAsync(id, seasonNumber);
        if (!exists)
            return ApiResults.NotFound("Poster not found");
        return ApiResults.PhysicalFile(path, "image/jpeg");
    }
    
    [HttpGet("{id}/season/{seasonNumber}/episode/{episodeNumber}/poster")]
    public async Task<IResult> GetEpisodePoster(int id, int seasonNumber, int episodeNumber)
    {
        var seasonId = _seasonRepository.GetSeasonIdAsync(seasonNumber, episodeNumber).Result;
        if (!seasonId.HasValue)
        {
            return ApiResults.NotFound("Season not found, to get episode poster");
        }
        
        var (exists, path) = await _tvEpisodeRepository.GetPosterPathAsync(id, seasonNumber, seasonId.Value, episodeNumber);
        if (!exists)
            return ApiResults.NotFound("Poster not found");
        return ApiResults.PhysicalFile(path, "image/jpeg");
    }
    
    [HttpGet("getTvRecommendation/{id}")]
    [ProducesResponseType(typeof(SuccessResponse<TmdbRecommendationsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetTvRecommendations(int id)
    {
        var recommendations = await _tvRecommendationRepository.GetTvRecommendationAsync(id);

        if (recommendations == null)
        {
            return ApiResults.NotFound($"No recommendations found for movie ID: {id}");
        }

        return ApiResults.Success(recommendations);
    }
}