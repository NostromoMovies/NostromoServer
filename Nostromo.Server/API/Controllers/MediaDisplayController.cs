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
        private readonly ITmdbService _tmdbService;

        public MediaDisplayController(
            IDatabaseService databaseService,
            ITmdbService tmdbService,
            ILogger<MediaDisplayController> logger)
        {
            _databaseService = databaseService;
            _tmdbService = tmdbService;
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

        [HttpGet("unrecognized-videos")]
        public async Task<IActionResult> GetUnrecognizedVideos()
        {
            try
            {
                var unrecognizedVideos = await _databaseService.GetVideosWithoutCrossRefAsync();

                return Ok(new
                {
                    Message = unrecognizedVideos.Any()
                        ? $"{unrecognizedVideos.Count} unrecognized videos found"
                        : "No unrecognized videos found",
                    Results = unrecognizedVideos.Select(video => new
                    {
                        video.VideoID,
                        video.FileName,
                        Timestamp = video.CreatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unrecognized videos");

                return StatusCode(500, new { Message = "An error occurred while fetching unrecognized videos" });
            }
        }

        [HttpGet("unrecognized-file-with-guesses/{videoID:int}")]
        public async Task<IActionResult> GetUnrecognizedFileWithGuesses(int videoID)
        {
            try
            {
                var video = await _databaseService.GetVideoByIdAsync(videoID);

                if (video == null)
                {
                    return NotFound(new { Message = "Unrecognized file not found for the given VideoID" });
                }

                var cleanTitle = video.FileName.Replace("_", " ").Split('.').FirstOrDefault();

                if (string.IsNullOrWhiteSpace(cleanTitle))
                {
                    return Ok(new
                    {
                        Message = "No suitable title found to search for guesses",
                        Result = new UnrecognizedFileResponse
                        {
                            VideoID = video.VideoID,
                            FileName = video.FileName,
                            Timestamp = video.CreatedAt,
                            Guesses = null
                        }
                    });
                }

                _logger.LogDebug("Searching TMDB for cleanTitle: {CleanTitle}", cleanTitle);

                var (results, totalResults) = await _tmdbService.SearchMoviesWithoutCaching(cleanTitle);

                var topGuesses = results.Take(10).Select(guess => new TmdbMovieResponse
                {
                    id = guess.id,
                    title = guess.title,
                    releaseDate = guess.releaseDate,
                    popularity = guess.popularity
                }).ToList();

                return Ok(new
                {
                    Message = topGuesses.Any()
                        ? $"Unrecognized file found with {totalResults} guesses, showing top {topGuesses.Count}"
                        : "Unrecognized file found but no guesses matched",
                    Result = new UnrecognizedFileResponse
                    {
                        VideoID = video.VideoID,
                        FileName = video.FileName,
                        Timestamp = video.CreatedAt,
                        Guesses = topGuesses
                    }
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument while processing VideoID: {VideoID}", videoID);
                return BadRequest(new { Message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, "No TMDB results found for VideoID: {VideoID}", videoID);
                return Ok(new
                {
                    Message = ex.Message,
                    Result = new UnrecognizedFileResponse
                    {
                        VideoID = videoID,
                        FileName = null,
                        Timestamp = null,
                        Guesses = null
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unrecognized file with guesses for VideoID: {VideoID}", videoID);
                return StatusCode(500, new { Message = "An error occurred while fetching unrecognized file with guesses" });
            }
        }

        [HttpPost("submit-match")]
        public async Task<IActionResult> SubmitMatch([FromBody] MatchRequest request)
        {
            if (request.VideoID <= 0 || request.TMDBMovieID <= 0)
            {
                return BadRequest(new { Message = "Invalid VideoID or TMDBMovieID." });
            }

            try
            {
                var video = await _databaseService.GetVideoByIdAsync(request.VideoID);
                if (video == null)
                {
                    return NotFound(new { Message = "Video not found." });
                }

                await _tmdbService.GetMovieById(request.TMDBMovieID);

                var crossRef = new CrossRefVideoTMDBMovie
                {
                    VideoID = request.VideoID,
                    TMDBMovieID = request.TMDBMovieID
                };

                await _databaseService.InsertCrossRefAsync(crossRef);

                return Ok(new { Message = "Match submitted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while submitting match for VideoID {VideoID} and TMDBMovieID {TMDBMovieID}",
                    request.VideoID, request.TMDBMovieID);
                return StatusCode(500, new { Message = "An error occurred while submitting the match." });
            }
        }
    }
}