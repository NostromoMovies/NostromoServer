using Microsoft.Extensions.Logging;
using Nostromo.Models;
using Nostromo.Server.API.Models;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Nostromo.Server.Services
{
    public interface IDatabaseService
    {
        Task<TMDBMovie> GetMovieAsync(int id);
        //Task InsertMovieAsync(TmdbMovieResponse movie);
        Task InsertGenreAsync(TmdbGenre genre);
        //Task<User> FindUserByUsernameAsync(string username);
        //Task CreateUserAsync(User userModel);
        Task<List<TMDBMovie>> SearchMoviesAsync(string title);
        Task<(int? tmdbId, int? seasonNum, int? episodeNum)> GetMovieIdByHashAsync(string hash);
        Task<int?> GetVideoIdByHashAsync(string fileHash);
        Task InsertCrossRefAsync(CrossRefVideoTMDBMovie crossRefModel);
        Task<List<TMDBMovie>> GetFilterMediaGenre(List<int> genresID);
        Task<List<TMDBMovie>> movieRatingsSorted();
        Task<List<Video>> GetAllVideosAsync();
        Task<bool> CheckCrossRefExistsAsync(int videoID, int tmdbMovieID);
        Task StoreMovieCastAsync(int movieId, List<TmdbCastMember> cast, bool isMovie, bool isTvShow, bool isTvEpisode);
        Task StoreMovieCrewAsync(int movieId, List<TmdbCrewMember> crew, bool isMovie, bool isTvShow, bool isTvEpisode);
        Task<List<TmdbCastMember>> GetCastByMovieIdAsync(int? movieId = null, int? tvShowId = null, int? episodeId = null);
        Task<List<TmdbCrewMember>> GetCrewByMovieIdAsync(int movieId);
        Task<DateTime> GetCreatedAtByVideoIdAsync(int? videoId);
        Task<Video?> GetVideoByIdAsync(int videoId);
        Task MarkVideoAsUnrecognizedAsync(int? videoId);
        Task MarkVideoAsRecognizedAsync(int? videoId);
        Task<List<Video>> GetAllUnrecognizedVideosAsync();
        Task InsertExampleHashAsync(string ed2kHash, int tmdbId, string title);
        Task StoreTmdbRecommendationsAsync(int movieId, TmdbRecommendation recommendation);
        
        Task StoreTvRecommendationsAsync(int showId, TvRecommendationResponse recommendation);
        Task<List<TMDBRecommendation>> GetRecommendationsByMovieIdAsync(int movieId);
        Task<List<TMDBMovie>> GetMoviesByUserAsync(string searchTerm, int maxRuntime, int sortBy);
        
        Task<List<TvShow>> GetTvShowsByUserAsync(string searchTerm, int minYear, int maxYear, int sortBy);
        Task<List<Genre>> getGenre();
        Task<int> GetMinYear();
        Task<TvShow> GetTvShowAsync(int id);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IUserRepository _userRepository;
        private readonly NostromoDbContext _context;
        private readonly ILogger<DatabaseService> _logger;
        private readonly ITvShowRepository _tvShowRepository;
        private readonly ITvEpisodeRepository _tvEpisodeRepository;

        public DatabaseService(
            IMovieRepository movieRepository,
            //IUserRepository userRepository,
            NostromoDbContext context,
            ILogger<DatabaseService> logger,
            ITvShowRepository tvShowRepository,
            ITvEpisodeRepository tvEpisodeRepository)
        {
            _movieRepository = movieRepository;
            //_userRepository = userRepository;
            _context = context;
            _logger = logger;
            _tvShowRepository = tvShowRepository;
            _tvEpisodeRepository = tvEpisodeRepository;
        }

        public async Task<(int?, int?, int?)> GetMovieIdByHashAsync(string hash)
        {
            // Log the input hash
            _logger.LogInformation("Searching for MovieID with hash: {InputHash}", hash);

            // Query the ExampleHashes table to find the corresponding MovieID
            var exampleHash = await _context.ExampleHash
                .FirstOrDefaultAsync(eh => eh.ED2K == hash);

            if (exampleHash != null)
            {
                // Log the found hash and MovieID
                _logger.LogInformation("Found matching hash: {DatabaseHash} with MovieID: {MovieID}", exampleHash.ED2K, exampleHash.TmdbId);
            }
            else
            {
                // Log that no match was found
                _logger.LogWarning("No matching hash found for: {InputHash}", hash);
            }

            return (exampleHash?.TmdbId, exampleHash?.SeasonNo, exampleHash?.EpisodeNo); // Return null if not found
        }

        public async Task<int?> GetVideoIdByHashAsync(string fileHash)
        {
            return await _context.Videos
                .Where(v => v.ED2K == fileHash || v.MD5 == fileHash || v.SHA1 == fileHash || v.CRC32 == fileHash)
                .Select(v => (int?)v.VideoID) // Cast to nullable int to handle cases where no match is found
                .FirstOrDefaultAsync();
        }

        public async Task<TMDBMovie> GetMovieAsync(int id)
        {
            return await _movieRepository.GetByIdAsync(id);
        }
        
        public async Task<TvShow> GetTvShowAsync(int id)
        {
            return await _tvShowRepository.GetByIdAsync(id);
        }

        //public async Task InsertMovieAsync(TmdbMovieResponse movieModel)
        //{
        //    try
        //    {
        //        var movie = new TMDBMovie
        //        {
        //            MovieID = movieModel.id,
        //            Title = movieModel.title,
        //            OriginalTitle = movieModel.originalTitle,
        //            OriginalLanguage = movieModel.OriginalLanguage,
        //            Overview = movieModel.overview,
        //            PosterPath = movieModel.posterPath,
        //            BackdropPath = movieModel.backdropPath,
        //            ReleaseDate = movieModel.releaseDate,
        //            IsAdult = movieModel.adult,
        //            Popularity = movieModel.popularity, // Fixed float to decimal conversion
        //            VoteCount = movieModel.voteCount,
        //            VoteAverage = movieModel.voteAverage, // Fixed float to decimal conversion
        //            Runtime = movieModel.runtime ?? 0 // Fixed nullable int conversion with default value
        //        };

        //        // Get or create genres
        //        if (movieModel.genreIds != null && movieModel.genreIds.Any())
        //        {
        //            var genreIds = movieModel.genreIds.Where(id => id != 0).ToList();
        //            var genres = await _context.Genres
        //                .Where(g => genreIds.Contains(g.GenreID))
        //                .ToListAsync();

        //            foreach (var genre in genres)
        //            {
        //                movie.Genres.Add(genre);
        //            }
        //        }

        //        await _movieRepository.AddAsync(movie);
        //        _logger.LogInformation("Successfully inserted movie: {Title} (ID: {Id})", movie.Title, movie.MovieID);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error inserting movie {Title} (ID: {Id})", movieModel.title, movieModel.id);
        //        throw;
        //    }
        //}

        public async Task InsertGenreAsync(TmdbGenre genreModel)
        {
            try
            {
                var genre = new Genre
                {
                    GenreID = genreModel.id,
                    Name = genreModel.name
                };

                var existingGenre = await _context.Genres.FindAsync(genre.GenreID);
                if (existingGenre == null)
                {
                    await _context.Genres.AddAsync(genre);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully inserted genre: {Name} (ID: {Id})", genre.Name, genre.GenreID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting genre {Name} (ID: {Id})", genreModel.name, genreModel.id);
                throw;
            }
        }

        public async Task InsertCrossRefAsync(CrossRefVideoTMDBMovie crossRefModel)
        {
            try
            {
                await _context.CrossRefVideoTMDBMovies.AddAsync(crossRefModel);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully inserted cross-reference for TMDBMovieID={TMDBMovieID} and VideoID={VideoID}",
                    crossRefModel.TMDBMovieID, crossRefModel.VideoID);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while inserting cross-reference for TMDBMovieID={TMDBMovieID} and VideoID={VideoID}",
                    crossRefModel.TMDBMovieID, crossRefModel.VideoID);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while inserting cross-reference for TMDBMovieID={TMDBMovieID} and VideoID={VideoID}",
                    crossRefModel.TMDBMovieID, crossRefModel.VideoID);
                throw;
            }
        }


        //public async Task<User> FindUserByUsernameAsync(string username)
        //{
        //    return await _userRepository.FindByUsernameAsync(username);
        //}

        //// IF MERGE CONFLICT ACCEPT THIS ONE
        //public async Task CreateUserAsync(Users userModel)
        //{
        //    try
        //    {
        //        var user = new User
        //        {
        //            Username = userModel.username,
        //            PasswordHash = userModel.passwordHash,
        //            Salt = userModel.salt
        //        };
        //        await _userRepository.AddAsync(user);  // Changed from CreateUserAsync to AddAsync
        //        _logger.LogInformation("Successfully created user: {Username}", user.Username);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating user {Username}", userModel.username);
        //        throw;
        //    }
        //}

        public async Task<List<TMDBMovie>> SearchMoviesAsync(string title)
        {
            try
            {
                var movies = await _movieRepository.SearchAsync(title);
                return movies.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies with title: {Title}", title);
                throw;
            }
        }
        public async Task<List<TMDBMovie>> GetFilterMediaGenre(List<int> genresID)
        {
            try
            {

                var allMovies = await _movieRepository.SearchGenreAsync(genresID);

                // Filter duplicates based on TMDBID
                var filteredMovies = allMovies
                    .GroupBy(movie => movie.TMDBID)
                    .Select(group => group.First())
                    .ToList();

                return filteredMovies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while filtering movies by genres.");
                throw;
            }
        }
        public async Task<List<TMDBMovie>> movieRatingsSorted()
        {

            try
            {

                var allMovies = await _movieRepository.SortMovieByRatings();


                return allMovies.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while grabbing movie ratings.");
                throw;
            }



        }
        public async Task<List<Video>> GetAllVideosAsync()
        {
            return await _context.Videos.ToListAsync();
        }

        public async Task<bool> CheckCrossRefExistsAsync(int videoID, int tmdbMovieID)
        {
            return await _context.CrossRefVideoTMDBMovies
                .AnyAsync(x => x.VideoID == videoID && x.TMDBMovieID == tmdbMovieID);
        }

        public async Task StoreMovieCastAsync(int mediaId, List<TmdbCastMember> cast, bool isMovie, bool isTvShow, bool isTvEpisode)
        {
            // Determine whether it's a movie, TV show, or episode
            TMDBMovie? movie = null;
            TvShow? tvShow = null;
            Episode? episode = null;

            if (isMovie)
            {
                movie = await _context.Movies.FirstOrDefaultAsync(m => m.MovieID == mediaId);
                if (movie == null)
                {
                    _logger.LogWarning("Movie with ID {MediaId} not found in database, skipping cast storage", mediaId);
                    return;
                }
            }
            else if (isTvShow)
            {
                tvShow = await _context.TvShows.FirstOrDefaultAsync(t => t.TvShowID == mediaId);
                if (tvShow == null)
                {
                    _logger.LogWarning("TV Show with ID {MediaId} not found in database, skipping cast storage", mediaId);
                    return;
                }
            }
            else if (isTvEpisode)
            {
                episode = await _context.Episodes.FirstOrDefaultAsync(e => e.EpisodeID == mediaId);
                if (episode == null)
                {
                    _logger.LogWarning("TV Episode with ID {MediaId} not found in database, skipping cast storage", mediaId);
                    return;
                }
            }

            foreach (var castMember in cast)
            {
                // Check if the person already exists
                var tmdbPerson = await _context.People.FirstOrDefaultAsync(p => p.TMDBID == castMember.id);
                if (tmdbPerson == null)
                {
                    tmdbPerson = new TMDBPerson
                    {
                        TMDBID = castMember.id,
                        EnglishName = castMember.name,
                        ProfilePath = castMember.profile_path,
                        CreatedAt = DateTime.UtcNow,
                        LastUpdatedAt = DateTime.UtcNow
                    };
                    await _context.People.AddAsync(tmdbPerson);
                    await _context.SaveChangesAsync();
                }

                // Create cast record
                var movieCast = new TMDBMovieCast
                {
                    TMDBPersonID = tmdbPerson.TMDBPersonID,
                    TMDBMovieID = isMovie ? movie?.MovieID : null,
                    TMDBTvShowID = isTvShow ? tvShow?.TvShowID : null,
                    TMDBTvEpisodeID = isTvEpisode ? episode?.EpisodeID : null,
                    Adult = castMember.adult,
                    Gender = castMember.gender,
                    TmdbCastMemberId = castMember.id != 0 ? castMember.id : null,
                    KnownForDepartment = castMember.known_for_department,
                    Name = castMember.name,
                    OriginalName = castMember.original_name,
                    Popularity = castMember.popularity,
                    ProfilePath = castMember.profile_path,
                    CastId = castMember.cast_id,
                    Character = castMember.character,
                    CreditID = castMember.credit_id,
                    Order = castMember.order
                };

                await _context.MovieCasts.AddAsync(movieCast);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Stored {Count} cast members for {MediaType} ID {MediaId}", cast.Count, isMovie ? "Movie" : isTvShow ? "TV Show" : "TV Episode", mediaId);
        }


        public async Task StoreMovieCrewAsync(int movieId, List<TmdbCrewMember> crew, bool isMovie, bool isTvShow, bool isTvEpisode)
        {
            TMDBMovie? movie = null;
            TvShow? tvShow = null;
            Episode? episode = null;
            var mediaType = "";
            if (isMovie)
            {
                movie = await _context.Movies.FirstOrDefaultAsync(m => m.MovieID == movieId);
                mediaType = "Movie";
                if (movie == null)
                {
                    _logger.LogWarning("Movie with ID {MediaId} not found in database, skipping cast storage", movieId);
                    return;
                }
            }
            else if (isTvShow)
            {
                tvShow = await _context.TvShows.FirstOrDefaultAsync(t => t.TvShowID == movieId);
                mediaType = "TV Show";
                if (tvShow == null)
                {
                    _logger.LogWarning("TV Show with ID {MediaId} not found in database, skipping cast storage", movieId);
                    return;
                }
            }
            else if (isTvEpisode)
            {
                episode = await _context.Episodes.FirstOrDefaultAsync(e => e.EpisodeID == movieId);
                mediaType = "TV Episode";
                if (episode == null)
                {
                    _logger.LogWarning("TV Episode with ID {MediaId} not found in database, skipping cast storage", movieId);
                    return;
                }
            }
            _logger.LogInformation("Creating new crew for {MediaType} ID {MediaId}", mediaType, movieId);
            foreach (var crewMember in crew)
            {
                var tmdbPerson = await _context.People.FirstOrDefaultAsync(p => p.TMDBID == crewMember.id);
                if (tmdbPerson == null)
                {
                    tmdbPerson = new TMDBPerson
                    {
                        TMDBID = crewMember.id,
                        EnglishName = crewMember.name,
                        ProfilePath = crewMember.profile_path,
                        CreatedAt = DateTime.UtcNow,
                        LastUpdatedAt = DateTime.UtcNow
                    };
                    await _context.People.AddAsync(tmdbPerson);
                    await _context.SaveChangesAsync();
                }

                var movieCrew = new TMDBMovieCrew
                {
                    TMDBPersonID = tmdbPerson.TMDBPersonID,
                    TMDBMovieID = isMovie ? movie?.MovieID: null,
                    TMDBTvShowID = isTvShow ? tvShow?.TvShowID : null,
                    TMDBTvEpisodeID = isTvEpisode ? episode?.EpisodeID : null,
                    Adult = crewMember.adult,
                    Gender = crewMember.gender,
                    Id = crewMember.id,
                    KnownForDepartment = crewMember.known_for_department,
                    Name = crewMember.name,
                    OriginalName = crewMember.original_name,
                    Popularity = crewMember.popularity,
                    ProfilePath = crewMember.profile_path,
                    CreditID = crewMember.credit_id,
                    Department = crewMember.department,
                    Job = crewMember.job
                };

                await _context.MovieCrews.AddAsync(movieCrew);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Stored {Count} crew members for {MediaType} ID {MovieId}", mediaType, crew.Count, movieId);
        }

        public async Task<List<TmdbCastMember>> GetCastByMovieIdAsync(int? movieId = null, int? tvShowId = null, int? episodeId = null)
        {
            var query = _context.MovieCasts.AsQueryable();

            if (movieId.HasValue)
            {
                query = query.Where(mc => mc.TMDBMovieID == movieId);
            }
            else if (tvShowId.HasValue)
            {
                query = query.Where(mc => mc.TMDBTvShowID == tvShowId);
            }
            else if (episodeId.HasValue)
            {
                query = query.Where(mc => mc.TMDBTvEpisodeID == episodeId);
            }
            else
            {
                return new List<TmdbCastMember>(); // No valid ID provided
            }

            return await query
                .Select(mc => new TmdbCastMember
                {
                    id = mc.TmdbCastMemberId ?? 0,  // Ensure non-null values
                    name = mc.Name,
                    original_name = mc.OriginalName,
                    character = mc.Character,
                    credit_id = mc.CreditID,
                    cast_id = mc.CastId,
                    profile_path = mc.ProfilePath,
                    popularity = mc.Popularity,
                    gender = mc.Gender,
                    known_for_department = mc.KnownForDepartment,
                    adult = mc.Adult,
                    order = mc.Order ?? 0
                })
                .ToListAsync();
        }


        public async Task<List<TmdbCrewMember>> GetCrewByMovieIdAsync(int movieId)
        {
            return await _context.MovieCrews
                .Where(mc => mc.TMDBMovieID == movieId)
                .Select(mc => new TmdbCrewMember
                {
                    id = mc.Id,
                    name = mc.Name,
                    original_name = mc.OriginalName,
                    credit_id = mc.CreditID,
                    profile_path = mc.ProfilePath,
                    popularity = mc.Popularity,
                    gender = mc.Gender,
                    known_for_department = mc.KnownForDepartment,
                    department = mc.Department,
                    job = mc.Job,
                    adult = mc.Adult
                })
                .ToListAsync();
        }

        public async Task<DateTime> GetCreatedAtByVideoIdAsync(int? videoId)
        {
            _logger.LogInformation("Retrieving CreatedAt for VideoID: {VideoId}", videoId);

            var createdAt = await _context.Videos
                .Where(v => v.VideoID == videoId)
                .Select(v => (DateTime?)v.CreatedAt)
                .FirstOrDefaultAsync();

            if (createdAt == null)
            {
                _logger.LogError("No video found with VideoID: {VideoId}", videoId);
                throw new KeyNotFoundException($"No video found with VideoID: {videoId}");
            }

            _logger.LogInformation("Found CreatedAt: {CreatedAt} for VideoID: {VideoId}", createdAt, videoId);
            return createdAt.Value;
        }

        public async Task<Video?> GetVideoByIdAsync(int videoId)
        {
            try
            {
                return await _context.Videos
                    .Where(v => v.VideoID == videoId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video with ID {VideoID}", videoId);
                throw;
            }
        }

        public async Task MarkVideoAsUnrecognizedAsync(int? videoId)
        {
            var video = await _context.Videos.FindAsync(videoId);
            if (video != null && video.IsRecognized)
            {
                video.IsRecognized = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated VideoID {VideoID} as unrecognized.", videoId);
            }
        }

        public async Task MarkVideoAsRecognizedAsync(int? videoId)
        {
            var video = await _context.Videos.FindAsync(videoId);
            if (video != null && !video.IsRecognized)
            {
                video.IsRecognized = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated VideoID {VideoID} as recognized.", videoId);
            }
        }

        public async Task<List<Video>> GetAllUnrecognizedVideosAsync()
        {
            return await _context.Videos
                .Where(v => !v.IsRecognized)
                .ToListAsync();
        }

        public async Task InsertExampleHashAsync(string ed2kHash, int tmdbId, string title)
        {
            try
            {
                var exampleHash = new ExampleHash
                {
                    ED2K = ed2kHash,
                    TmdbId = tmdbId,
                    Title = title
                };

                await _context.ExampleHash.AddAsync(exampleHash);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully inserted ExampleHash entry: {ED2K}, {TMDBID}, {Title}", ed2kHash, tmdbId, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting ExampleHash entry: {ED2K}, {TMDBID}, {Title}", ed2kHash, tmdbId, title);
                throw;
            }
        }

        public async Task StoreTmdbRecommendationsAsync(int movieId, TmdbRecommendation recommendation)
        {
            try
            {
                var recommendationEntity = new TMDBRecommendation
                {
                    Id = recommendation.id,
                    TMDBMovieID = movieId,
                    Title = recommendation.title,
                    OriginalTitle = recommendation.originalTitle,
                    Overview = recommendation.overview,
                    PosterPath = recommendation.posterPath,
                    BackdropPath = recommendation.backdropPath,
                    MediaType = "movie",
                    Adult = recommendation.adult,
                    OriginalLanguage = recommendation.OriginalLanguage,
                    GenreIds = string.Join(",", recommendation.genreIds),
                    Popularity = recommendation.popularity,
                    ReleaseDate = recommendation.releaseDate,
                    Video = recommendation.video,
                    VoteAverage = recommendation.voteAverage,
                    VoteCount = recommendation.voteCount
                };

                var existingRecommendation = await _context.Recommendations
                    .FirstOrDefaultAsync(r => r.Id == recommendationEntity.Id);

                if (existingRecommendation == null)
                {
                    await _context.Recommendations.AddAsync(recommendationEntity);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Stored TMDB recommendation: {Title} (ID: {Id})", recommendation.title, recommendation.id);
                }
                else
                {
                    _logger.LogWarning("TMDB recommendation already exists: {Title} (ID: {Id})", recommendation.title, recommendation.id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing TMDB recommendation: {Title} (ID: {Id})", recommendation.title, recommendation.id);
                throw;
            }
        }

        public async Task StoreTvRecommendationsAsync(int showId, TvRecommendationResponse recommendation)
        {
            try
            {
                var recommendationEntity = new TvRecommendation
                {
                    Id = recommendation.Id,
                    ShowId = showId,
                    Name = recommendation.Name,
                    Adult = recommendation.Adult,
                    BackdropPath = recommendation.backdropPath,
                    OriginalName = recommendation.OriginalName,
                    Overview = recommendation.Overview,
                    PosterPath = recommendation.PosterPath,
                    MediaType = recommendation.MediaType,
                    VoteAverage = recommendation.VoteAverage,
                    VoteCount = recommendation.VoteCount,
                    firstAirDate = recommendation.firstAirDate,
                    Popularity = recommendation.Popularity,
                        
                }; 

                var existingRecommendation = await _context.Recommendations
                    .FirstOrDefaultAsync(r => r.Id == recommendationEntity.Id);

                if (existingRecommendation == null)
                {
                    await _context.TvRecommendations.AddAsync(recommendationEntity);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Stored TMDB recommendation: {Name} (ID: {Id})", recommendation.Name, recommendation.Id);
                }
                else
                {
                    _logger.LogWarning("TMDB recommendation already exists: {Name} (ID: {Id})", recommendation.Name, recommendation.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing TMDB recommendation: {Name} (ID: {Id})", recommendation.Name, recommendation.Id);
                throw;
            }
        }
        public async Task<List<TMDBRecommendation>> GetRecommendationsByMovieIdAsync(int movieId)
        {
            try
            {
                _logger.LogInformation("Retrieving recommendations for Movie ID: {MovieId}", movieId);

                var recommendations = await _context.Recommendations
                    .Where(r => r.TMDBMovieID == movieId)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} recommendations for Movie ID: {MovieId}", recommendations.Count, movieId);

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recommendations for Movie ID: {MovieId}", movieId);
                throw;
            }
        }

        public async Task<List<TMDBMovie>> GetMoviesByUserAsync(string searchTerm, int maxRuntime, int sortBy)
        {
            // recently added -- good
            if (sortBy == 3)
            {
                return await _context.CrossRefVideoTMDBMovies
                    .Include(c => c.TMDBMovie)
                    .Where(c =>
                        (string.IsNullOrEmpty(searchTerm) || c.TMDBMovie.Title.ToLower().Contains(searchTerm.ToLower())) &&
                        (maxRuntime == null || c.TMDBMovie.Runtime <= maxRuntime))
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => c.TMDBMovie)
                    .ToListAsync();
            }
            // alphabetical -- good
            else if (sortBy == 1)
            {
                return await _context.Movies
                    .Where(c =>
                        (string.IsNullOrEmpty(searchTerm) || c.Title.ToLower().Contains(searchTerm.ToLower())) &&
                        (maxRuntime == null || c.Runtime <= maxRuntime))
                    .OrderBy(c => c.Title.ToLower())
                    .ToListAsync();
            }
            // highest rated -- good
            else if (sortBy == 2)
            {
                return await _context.Movies
                    .Where(c =>
                        (string.IsNullOrEmpty(searchTerm) || c.Title.ToLower().Contains(searchTerm.ToLower())) &&
                        (maxRuntime == null || c.Runtime <= maxRuntime))
                    .OrderByDescending(c => c.VoteAverage)
                    .ToListAsync();
            }
            // popularity -- good
            else if (sortBy == 0)
            {
                return await _context.Movies
                    .Where(c =>
                        (string.IsNullOrEmpty(searchTerm) || c.Title.ToLower().Contains(searchTerm.ToLower())) &&
                        (maxRuntime == null || c.Runtime <= maxRuntime))
                    .OrderByDescending(c => c.Popularity)
                    .ToListAsync();
            }



            return await _context.Movies.ToListAsync();
        }

        public async Task<List<TvShow>> GetTvShowsByUserAsync(string searchTerm, int minYear, int maxYear, int sortBy)
        {

            var result = _context.TvShows
                    .Where(c =>
                        (string.IsNullOrEmpty(searchTerm) || c.OriginalName.ToLower().Contains(searchTerm.ToLower()))); /*&&
                        (maxYear == 0 || DateTime.Parse(c.FirstAirDate).Year <= maxYear) &&
                        (minYear == 3000 || DateTime.Parse(c.FirstAirDate).Year >= minYear));*/

            result = sortBy switch
            {
                0 => result.OrderByDescending(c => c.Popularity),
                1 => result.OrderByDescending(c => c.OriginalName.ToLower()),
                2 => result.OrderByDescending(c => c.VoteAverage),
                _ => result.OrderByDescending(c => c.Popularity),
            };
            
            return await result.ToListAsync();
        }
        public async Task<List<Genre>> getGenre()
        {

            return await _context.Genres.ToListAsync();
        }

        public async Task<int> GetMinYear()
        {
            var years = await _context.Movies
                .Where(m => m.ReleaseDate != null)
                .Select(m => DateTime.Parse(m.ReleaseDate).Year)
                .ToListAsync();

            int minYear = years.Any() ? years.Min() : DateTime.Now.Year;

            return minYear;
        }

        public async Task<String?> getMediaType(int id)
        {
            return await _context.MediaTypes
                .Where(m => m.MediaTmDBID == id)
                .Select(m => m.MediaTypeName)
                .FirstOrDefaultAsync();
        }
        
        
    }
}