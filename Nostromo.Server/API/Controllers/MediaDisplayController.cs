using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nostromo.Models;
using Nostromo.Server.Services;
using Nostromo.Server.Database;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaDisplayController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<MediaDisplayController> _logger;

        public MediaDisplayController(
            IDatabaseService databaseService,
            ILogger<MediaDisplayController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        [HttpGet("searchMedia")]
        public async Task<ActionResult<IEnumerable<TmdbMovieResponse>>> SearchMedia([FromQuery] string mediaName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mediaName))
                {
                    return BadRequest(new { Message = "Search term is required" });
                }

                _logger.LogInformation("Searching for media with name: {MediaName}", mediaName);

                var movies = await _databaseService.SearchMoviesAsync(mediaName);

                if (!movies.Any())
                {
                    return Ok(new
                    {
                        Message = "No movies found matching the search criteria",
                        Results = Array.Empty<TmdbMovieResponse>()
                    });
                }

                // Convert database entities to API model
                var results = movies.Select(movie => new TmdbMovieResponse
                {
                    id = movie.MovieID,
                    title = movie.Title,
                    originalTitle = movie.OriginalTitle,
                    overview = movie.Overview,
                    posterPath = movie.PosterPath,
                    backdropPath = movie.BackdropPath,
                    releaseDate = movie.ReleaseDate,
                    adult = movie.IsAdult,
                    popularity = Convert.ToSingle(movie.Popularity),
                    voteCount = movie.VoteCount,
                    voteAverage = Convert.ToSingle(movie.VoteAverage),
                    runtime = movie.Runtime,
                    genreIds = movie.Genres?.Select(g => g.GenreID).ToList() ?? new List<int>()
                }).ToList();

                _logger.LogInformation("Found {Count} movies matching search term: {MediaName}",
                    results.Count, mediaName);

                return Ok(new
                {
                    Message = $"Found {results.Count} results",
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for media with name: {MediaName}", mediaName);
                return StatusCode(500, new { Message = "An error occurred while searching for media" });
            }
        }

        // You might want to add other endpoints for getting movie details, genres, etc.
        [HttpGet("{id}")]
        public async Task<ActionResult<TmdbMovieResponse>> GetMovie(int id)
        {
            try
            {
                var movie = await _databaseService.GetMovieAsync(id);

                if (movie == null)
                {
                    return NotFound(new { Message = $"Movie with ID {id} not found" });
                }

                var result = new TmdbMovieResponse
                {
                    id = movie.MovieID,
                    title = movie.Title,
                    originalTitle = movie.OriginalTitle,
                    overview = movie.Overview,
                    posterPath = movie.PosterPath,
                    backdropPath = movie.BackdropPath,
                    releaseDate = movie.ReleaseDate,
                    adult = movie.IsAdult,
                    popularity = Convert.ToSingle(movie.Popularity),
                    voteCount = movie.VoteCount,
                    voteAverage = Convert.ToSingle(movie.VoteAverage),
                    runtime = movie.Runtime,
                    genreIds = movie.Genres?.Select(g => g.GenreID).ToList() ?? new List<int>()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movie with ID: {MovieId}", id);
                return StatusCode(500, new { Message = "An error occurred while retrieving the movie" });
            }
        }
       
   
        [HttpGet("filtered-genre")]
        public async Task<ActionResult<List<TmdbMovieResponse>>> GetFilteredGenre([FromQuery] List<int> genres)
        {
            try
            {
                var movies = await _databaseService.GetFilterMediaGenre(genres);
                if (movies == null)
                {
                    return NotFound(new { Message = $"Movie with genreID {genres} not found" });
                }
                List<TmdbMovieResponse> tmbdMovie = new List<TmdbMovieResponse>();
                foreach (var movie in movies)
                {
                    var result = new TmdbMovieResponse
                    {
                        id = movie.MovieID,
                        title = movie.Title,
                        originalTitle = movie.OriginalTitle,
                        overview = movie.Overview,
                        posterPath = movie.PosterPath,
                        backdropPath = movie.BackdropPath,
                        releaseDate = movie.ReleaseDate,
                        adult = movie.IsAdult,
                        popularity = Convert.ToSingle(movie.Popularity),
                        voteCount = movie.VoteCount,
                        voteAverage = Convert.ToSingle(movie.VoteAverage),
                        runtime = movie.Runtime,
                        genreIds = movie.Genres?.Select(g => g.GenreID).ToList() ?? new List<int>()
                    };
                    tmbdMovie.Add(result);
                }


                return Ok(tmbdMovie);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { Message = "An error occurred while retrieving the movie" });
            }
        }
        [HttpGet("filtered-highestRatings")]
        public async Task<ActionResult<List<TmdbMovieResponse>>> HighestRatings()
        {

            try
            {
                List<TMDBMovie> tmbdMovie = new List<TMDBMovie>();
                tmbdMovie = await _databaseService.movieRatingsSorted();
             
                


                return Ok(tmbdMovie);
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { Message = "An error occurred while retrieving the movie" });
            }

        }
    }
}