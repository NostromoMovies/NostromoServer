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
                if ( string.IsNullOrWhiteSpace(mediaName) )
                    return ApiResults.BadRequest("Search term is required" );

                _logger.LogInformation("Searching for media with name: {MediaName}", mediaName);

                var movies = await _databaseService.SearchMoviesAsync(mediaName);

                if (!movies.Any())
                    return ApiResults.SuccessCollection<TmdbMovieResponse>(
                        Array.Empty<TmdbMovieResponse>());

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
                    genreIds = movie.Genres?
                                        .Select(g => new TmdbGenre { id = g.GenreID, name = g.Name })
                                        .ToList() ?? new List<TmdbGenre>()
                })
                .ToList();

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
                    genreIds = movie.Genres?

                            .Select(g => new TmdbGenre { id = g.GenreID, name = g.Name })
                            .ToList() ?? new List<TmdbGenre>()

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
                        genreIds = movie.Genres?
                            .Select(g => new TmdbGenre { id = g.GenreID, name = g.Name })
                            .ToList() ?? new List<TmdbGenre>()
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
                var unrecognizedVideos = await _databaseService.  GetAllUnrecognizedVideosAsync();

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

                _logger.LogInformation("Scheduling TMDB metadata job for TMDBMovie {TMDBMovieID}.", request.TMDBMovieID);

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

                _logger.LogInformation("Waiting for TMDB metadata to be available before proceeding...");

                TMDBMovie? movieDetails = null;
                int retryCount = 0;
                while (movieDetails == null && retryCount < 60)
                {
                    await Task.Delay(1000);
                    movieDetails = await _databaseService.GetMovieAsync(request.TMDBMovieID);
                    retryCount++;
                }

                if (movieDetails == null)
                {
                    _logger.LogWarning("Could not fetch TMDB metadata for MovieID: {MovieID}", request.TMDBMovieID);
                    return BadRequest("Failed to retrieve TMDB metadata.");
                }

                _logger.LogInformation("Fetched TMDB metadata for {Title} (ID: {MovieID})", movieDetails.Title, request.TMDBMovieID);

                _logger.LogInformation("Scheduling DownloadMovieMetadataJob for Video {VideoID}.", request.VideoID);

                var metadataJobId = Guid.NewGuid().ToString();
                var metadataJobKey = new JobKey(metadataJobId, "ConsolidateGroup");

                var metadataJob = JobBuilder.Create<DownloadDirectMovieMetadataJob>()
                    .UsingJobData(DownloadDirectMovieMetadataJob.HASH_KEY, video.ED2K)
                    .UsingJobData("TMDBMovieID", request.TMDBMovieID)
                    .WithIdentity(metadataJobKey)
                    .Build();

                var metadataTrigger = TriggerBuilder.Create()
                    .StartNow()
                    .WithIdentity(new TriggerKey(Guid.NewGuid().ToString(), "ConsolidateGroup"))
                    .Build();

                await _scheduler.ScheduleJob(metadataJob, metadataTrigger);

                _logger.LogInformation("Successfully scheduled metadata jobs for Video {VideoID} and TMDBMovie {TMDBMovieID}.",
                    request.VideoID, request.TMDBMovieID);

                return Ok(new
                {
                    Message = $"Metadata jobs scheduled for Video {request.VideoID} and TMDB movie {request.TMDBMovieID}."
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

        [HttpGet("movie/{id}/recommendations")]
        [ProducesResponseType(typeof(SuccessResponse<List<TMDBRecommendation>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IResult> GetRecommendationsByMovieId(int id)
        {
            var recommendations = await _databaseService.GetRecommendationsByMovieIdAsync(id);

            if (recommendations == null || recommendations.Count == 0)
            {
                return ApiResults.NotFound($"No recommendations found for movie ID: {id}");
            }

            return ApiResults.SuccessCollection(recommendations);
        }

        [HttpGet("getMovies")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TMDBMovie>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> GetFilteredMovies(
            [FromQuery] string query = null,
            [FromQuery] int runtime = 300,
            [FromQuery] int searchTerm = 0,
            [FromQuery] string minYear = "0",
            [FromQuery] string  maxYear = "3000",
            [FromQuery] List<string> filterGenre = null)
        {
            try
            {
                var tmdbMovies = await _databaseService.GetMoviesByUserAsync(query, runtime, searchTerm,minYear, maxYear,filterGenre);

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

        [HttpGet("getshows")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TvShowDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> GetFilteredTvShows(
            [FromQuery] string query = null,
            [FromQuery] int searchTerm = 0,
            [FromQuery] int minYear = 0,
            [FromQuery] int maxYear = 3000,
            [FromQuery] List<string> filterGenre = null)
        {
            try
            {
                var shows = await _databaseService.GetTvShowsByUserAsync(query, minYear, maxYear, searchTerm,filterGenre);

                var response = new
                {
                    data = new
                    {
                        items = shows
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
        
        [HttpGet("getMovieCount")]
        public async Task<ActionResult<int>> MovieCount()
        {
            return await _databaseService.GetMovieCount();
        }
        
        [HttpGet("getMovieCountGenre")]
        public async Task<ActionResult<IEnumerable<GenreCounter>>> MovieCountGenre()
        {
            return await _databaseService.GetGenreMovieCount();
        }

        [HttpGet("{id}/getVideoID")]
        public async Task<ActionResult<int>> GetVideoiD(int id)
        {
            var videoId = await _databaseService.GetVideoID(id);
            return videoId;
        }
    }
}