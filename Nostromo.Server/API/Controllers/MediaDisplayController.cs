using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nostromo.Models;
using Nostromo.Server.Services;
using Nostromo.Server.Database;
using Nostromo.Server.API.Models;
using System;
using Nostromo.Server.Scheduling.Jobs;
using Quartz;
using System.Threading.Tasks;


namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaDisplayController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<MediaDisplayController> _logger;
        private readonly IScheduler _scheduler;

        public MediaDisplayController(
            IDatabaseService databaseService,
            ILogger<MediaDisplayController> logger,
            IScheduler scheduler)
        {
            _databaseService = databaseService;
            _logger = logger;
            _scheduler = scheduler;
        }

        [HttpGet("searchMedia")]
        public async Task<IResult> SearchMedia([FromQuery] string mediaName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mediaName))
                    return ApiResults.BadRequest("Search term is required");

                _logger.LogInformation("Searching for media with name: {MediaName}", mediaName);

                var movies = await _databaseService.SearchMoviesAsync(mediaName);

                if (!movies.Any())
                    return ApiResults.SuccessCollection<TmdbMovieResponse>([]);

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

                return ApiResults.SuccessCollection<TmdbMovieResponse>(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for media with name: {MediaName}", mediaName);
                return ApiResults.ServerError("An error occurred while searching for media");
            }
        }

        // You might want to add other endpoints for getting movie details, genres, etc.
        [HttpGet("{id}")]
        public async Task<IResult> GetMovie(int id)
        {
            try
            {
                var movie = await _databaseService.GetMovieAsync(id);

                if (movie == null)
                {
                    return ApiResults.NotFound($"Movie with ID {id} not found");
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

                return ApiResults.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movie with ID: {MovieId}", id);
                return ApiResults.ServerError("An error occurred while retrieving the movie");
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

        [HttpGet("unrecognized-movies")]
        public async Task<ActionResult<List<Video>>> GetUnrecognizedMovies()
        {
            try
            {
                var unrecognizedVideos = await _databaseService.GetAllUnrecognizedVideosAsync();

                if (!unrecognizedVideos.Any())
                {
                    return NotFound(new { Message = "No unrecognized movies found" });
                }

                return Ok(unrecognizedVideos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unrecognized movies");
                return StatusCode(500, new { Message = "An error occurred while retrieving unrecognized movies" });
            }
        }

        [HttpPost("linkMovie")]
        public async Task<IActionResult> LinkMovie([FromBody] LinkMovieRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request cannot be null.");

                if (request.VideoID <= 0 || request.TMDBMovieID <= 0)
                    return BadRequest("VideoID and TMDBMovieID must be greater than 0.");

                var video = await _databaseService.GetVideoByIdAsync(request.VideoID);
                if (video == null)
                {
                    return NotFound($"Video with ID {request.VideoID} not found.");
                }

                _logger.LogInformation("Scheduling metadata jobs for Video {VideoID} with TMDBMovie {TMDBMovieID}.",
                    request.VideoID, request.TMDBMovieID);

                var metadataJobId = Guid.NewGuid().ToString();
                var metadataJobKey = new JobKey(metadataJobId, "ConsolidateGroup");

                var metadataJob = JobBuilder.Create<DownloadMovieMetadataJob>()
                    .UsingJobData(DownloadMovieMetadataJob.HASH_KEY, video.ED2K)
                    .WithIdentity(metadataJobKey)
                    .Build();

                var metadataTrigger = TriggerBuilder.Create()
                    .StartNow()
                    .WithIdentity(new TriggerKey(Guid.NewGuid().ToString(), "ConsolidateGroup"))
                    .Build();

                await _scheduler.ScheduleJob(metadataJob, metadataTrigger);

                var tmdbJobId = Guid.NewGuid().ToString();
                var tmdbJobKey = new JobKey(tmdbJobId, "MetadataGroup");

                var tmdbJob = JobBuilder.Create<DownloadTMDBMetadataJob>()
                    .UsingJobData(DownloadTMDBMetadataJob.MOVIE_ID_KEY, request.TMDBMovieID)
                    .WithIdentity(tmdbJobKey)
                    .Build();

                var tmdbTrigger = TriggerBuilder.Create()
                    .StartNow()
                    .WithIdentity(new TriggerKey(Guid.NewGuid().ToString(), "MetadataGroup"))
                    .Build();

                await _scheduler.ScheduleJob(tmdbJob, tmdbTrigger);

                _logger.LogInformation(
                    "Successfully scheduled metadata jobs for Video {VideoID} and TMDBMovie {TMDBMovieID}.",
                    request.VideoID, request.TMDBMovieID);

                return Ok(new
                {
                    Message =
                        $"Metadata jobs scheduled for Video {request.VideoID} and TMDB movie {request.TMDBMovieID}."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling metadata jobs for Video {VideoID} and TMDBMovie {TMDBMovieID}",
                    request.VideoID, request.TMDBMovieID);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while scheduling metadata jobs.",
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
        [HttpGet("getMovies")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TMDBMovie>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> GetFilteredMovies(
            [FromQuery] string query = null,
            [FromQuery] int runtime = 300,
            [FromQuery] int searchTerm = 0,
            [FromQuery] int minYear = 0,
            [FromQuery] int maxYear = 3000)
        {
            try
            {
                var tmdbMovies = await _databaseService.GetMoviesByUserAsync(query, runtime, searchTerm);

                // Wrap the movies list inside { data: { items: [...] } }
                var response = new
                {
                    data = new
                    {
                        items = tmdbMovies
                    }
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.StackTrace,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "An error occurred while retrieving movies.",
                    extensions: new Dictionary<string, object>
                    {
                        { "Error", ex.Message }
                    }
                );
            }
        }


        [HttpGet("getGenres")]
        public async Task<ActionResult<IEnumerable<Genre>>> GetGenre()
        {
            
            return await _databaseService.getGenre();
        }
    
        
        [HttpGet("getYears")]
        public async Task<ActionResult<int>> GetYears()
        {
            return await _databaseService.GetMinYear(); 
        }

    }
}