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
        Task<int?> GetMovieIdByHashAsync(string hash);
        Task<int?> GetVideoIdByHashAsync(string fileHash);
        Task InsertCrossRefAsync(CrossRefVideoTMDBMovie crossRefModel);
        Task<List<TMDBMovie>> GetFilterMediaGenre(List<int> genresID);
        Task<List<TMDBMovie>> movieRatingsSorted();
        Task<List<Video>> GetAllVideosAsync();
        Task<bool> CheckCrossRefExistsAsync(int videoID, int tmdbMovieID);
        Task StoreMovieCastAsync(int movieId, List<TmdbCastMember> cast);
        Task StoreMovieCrewAsync(int movieId, List<TmdbCrewMember> crew);
        Task<List<TmdbCastMember>> GetCastByMovieIdAsync(int movieId);
        Task<List<TmdbCrewMember>> GetCrewByMovieIdAsync(int movieId);
        Task<DateTime> GetCreatedAtByVideoIdAsync(int? videoId);
        Task<Video?> GetVideoByIdAsync(int videoId);
        Task MarkVideoAsUnrecognizedAsync(int? videoId);
        Task MarkVideoAsRecognizedAsync(int? videoId);
        Task<List<Video>> GetAllUnrecognizedVideosAsync();
        Task InsertExampleHashAsync(string ed2kHash, int tmdbId, string title);
        Task StoreTmdbRecommendationsAsync(int movieId, TmdbRecommendation recommendation);
        Task<List<TMDBRecommendation>> GetRecommendationsByMovieIdAsync(int movieId);
        Task<List<TMDBMovie>> GetMoviesByUserAsync(string searchTerm, int maxRuntime, int sortBy,string minYear, string  maxYear,List<string> genreIds);
        Task<List<Genre>> getGenre();
        Task<int> GetMinYear();
        Task StoreMovieGenresAsync(int movieId, List<TmdbGenre> genres);
        Task<int> GetMovieCount();
        Task<List<GenreCounter>> GetGenreMovieCount();
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IUserRepository _userRepository;
        private readonly NostromoDbContext _context;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(
            IMovieRepository movieRepository,
            //IUserRepository userRepository,
            NostromoDbContext context,
            ILogger<DatabaseService> logger)
        {
            _movieRepository = movieRepository;
            //_userRepository = userRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<int?> GetMovieIdByHashAsync(string hash)
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

            return exampleHash?.TmdbId; // Return null if not found
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
                var existingGenre = await _context.Genres
                    .FirstOrDefaultAsync(g => g.GenreID == genreModel.id && g.Name == genreModel.name);

                if (existingGenre == null)
                {
                    var genre = new Genre
                    {
                        GenreID = genreModel.id,
                        Name = genreModel.name
                    };

                    await _context.Genres.AddAsync(genre);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully inserted genre: {Name} (TMDB ID: {Id})", genre.Name, genre.GenreID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting genre {Name} (TMDB ID: {Id})", genreModel.name, genreModel.id);
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

        public async Task StoreMovieCastAsync(int movieId, List<TmdbCastMember> cast)
        {
            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.MovieID == movieId);
            if (movie == null)
            {
                _logger.LogWarning("Movie with ID {MovieId} not found in database, skipping cast storage", movieId);
                return;
            }

            foreach (var castMember in cast)
            {
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

                var movieCast = new TMDBMovieCast
                {
                    TMDBPersonID = tmdbPerson.TMDBPersonID,
                    TMDBMovieID = movie.MovieID,
                    Adult = castMember.adult,
                    Gender = castMember.gender,
                    Id = castMember.id,
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
            _logger.LogInformation("Stored {Count} cast members for movie ID {MovieId}", cast.Count, movieId);
        }

        public async Task StoreMovieCrewAsync(int movieId, List<TmdbCrewMember> crew)
        {
            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.MovieID == movieId);
            if (movie == null)
            {
                _logger.LogWarning("Movie with ID {MovieId} not found in database, skipping cast storage", movieId);
                return;
            }

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
                    TMDBMovieID = movie.MovieID,
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
            _logger.LogInformation("Stored {Count} cast members for movie ID {MovieId}", crew.Count, movieId);
        }

        public async Task<List<TmdbCastMember>> GetCastByMovieIdAsync(int movieId)
        {
            return await _context.MovieCasts
                .Where(mc => mc.TMDBMovieID == movieId)
                .Select(mc => new TmdbCastMember
                {
                    id = mc.Id,
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

                    foreach (var genre in recommendation.genreIds ?? new List<TmdbGenre>())
                    {
                        var existingGenre = await _context.Genres
                            .FirstOrDefaultAsync(g => g.GenreID == genre.id && g.Name == genre.name);

                        if (existingGenre == null)
                        {
                            existingGenre = new Genre
                            {
                                GenreID = genre.id,
                                Name = genre.name
                            };
                            _context.Genres.Add(existingGenre);
                            await _context.SaveChangesAsync();
                        }

                        _context.Add(new RecommendationGenre
                        {
                            RecommendationID = recommendationEntity.RecommendationID,
                            GenreID = genre.id,
                            Name = genre.name
                        });
                    }

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

        public async Task<List<TMDBMovie>> GetMoviesByUserAsync(string searchTerm, int maxRuntime, int sortBy,string minYear, string maxYear,List<string> filterGenre)
        {
            // recently added -- good
            if (sortBy == 3)    
            {
                var movies = await _context.CrossRefVideoTMDBMovies
                    .Include(c => c.TMDBMovie)
                    .Where(c =>
                        (string.IsNullOrEmpty(searchTerm) || c.TMDBMovie.Title.ToLower().Contains(searchTerm.ToLower())) &&
                        (maxRuntime == null || c.TMDBMovie.Runtime <= maxRuntime))
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => c.TMDBMovie)
                    .ToListAsync();  // Fetch the movies into memory


                if (!string.IsNullOrEmpty(minYear))
                {
                    int minYearInt = int.Parse(minYear);
                    movies = movies.Where(c => DateTime.Parse(c.ReleaseDate).Year >= minYearInt).ToList();
                }

                if (!string.IsNullOrEmpty(maxYear))
                {
                    int maxYearInt = int.Parse(maxYear);
                    movies = movies.Where(c => DateTime.Parse(c.ReleaseDate).Year <= maxYearInt).ToList();
                }
                if (filterGenre != null && filterGenre.Any())
                {
                    var genreIds = new List<int>();
                    foreach (var genre in filterGenre)
                    {
                        if (int.TryParse(genre, out int id))
                        {
                            genreIds.Add(id);
                        }
                    }
    
                    if (genreIds.Any())
                    {
                        var movieIdsWithMatchingGenres = await _context.MovieGenres
                            .Where(mg => genreIds.Contains(mg.GenreID))
                            .Select(mg => mg.MovieID)
                            .Distinct()
                            .ToListAsync();

                        var movieIdSet = new HashSet<int>(movieIdsWithMatchingGenres);

                        movies = movies
                            .Where(movie => movieIdSet.Contains(movie.MovieID))
                            .ToList();
                    }
                }

                return movies;
            }
            // alphabetical -- good
            else if (sortBy == 1)
            {
                /*return await _context.Movies
                    .Where(c =>
                        (string.IsNullOrEmpty(searchTerm) || c.Title.ToLower().Contains(searchTerm.ToLower())) &&
                        (maxRuntime == null || c.Runtime <= maxRuntime))
                    .OrderBy(c => c.Title.ToLower())
                    .ToListAsync();*/
                
                var movies = await _context.Movies
                    .Where(c =>
                        (string.IsNullOrEmpty(searchTerm) || c.Title.ToLower().Contains(searchTerm.ToLower())) &&
                        (maxRuntime == null || c.Runtime <= maxRuntime))
                    .ToListAsync();  // Fetch the movies into memory


                if (!string.IsNullOrEmpty(minYear))
                {
                    int minYearInt = int.Parse(minYear);
                    movies = movies.Where(c => DateTime.Parse(c.ReleaseDate).Year >= minYearInt).ToList();
                }

                if (!string.IsNullOrEmpty(maxYear))
                {
                    int maxYearInt = int.Parse(maxYear);
                    movies = movies.Where(c => DateTime.Parse(c.ReleaseDate).Year <= maxYearInt).ToList();
                }

                return movies
                    .OrderBy(c => c.Title.ToLower())  
                    .ToList();


            }
            // highest rated -- good
            else if (sortBy == 2)
            {
                /*
                return await _context.Movies
                    .Where(c =>
                        (string.IsNullOrEmpty(searchTerm) || c.Title.ToLower().Contains(searchTerm.ToLower())) &&
                        (maxRuntime == null || c.Runtime <= maxRuntime))
                    .OrderByDescending(c => c.VoteAverage)
                    .ToListAsync();
                    */
                
                
                var movies = await  _context.Movies
                        .Where(c =>
                            (string.IsNullOrEmpty(searchTerm) || c.Title.ToLower().Contains(searchTerm.ToLower())) &&
                            (maxRuntime == null || c.Runtime <= maxRuntime))
                        .ToListAsync();  // Fetch the movies into memory
                
                if (!string.IsNullOrEmpty(minYear))
                {
                    int minYearInt = int.Parse(minYear);
                    movies = movies.Where(c => DateTime.Parse(c.ReleaseDate).Year >= minYearInt).ToList();
                }

                if (!string.IsNullOrEmpty(maxYear))
                {
                    int maxYearInt = int.Parse(maxYear);
                    movies = movies.Where(c => DateTime.Parse(c.ReleaseDate).Year <= maxYearInt).ToList();
                }
                return movies.OrderByDescending(c => c.VoteAverage).ToList();
                   
            }
            // popularity -- good
            else if (sortBy == 0)
            {
                var movies = await  _context.Movies
                    .Where(c =>
                        (string.IsNullOrEmpty(searchTerm) || c.Title.ToLower().Contains(searchTerm.ToLower())) &&
                        (maxRuntime == null || c.Runtime <= maxRuntime))
                    .ToListAsync();  // Fetch the movies into memory
                
                if (!string.IsNullOrEmpty(minYear))
                {
                    int minYearInt = int.Parse(minYear);
                    movies = movies.Where(c => DateTime.Parse(c.ReleaseDate).Year >= minYearInt).ToList();
                }

                if (!string.IsNullOrEmpty(maxYear))
                {
                    int maxYearInt = int.Parse(maxYear);
                    movies = movies.Where(c => DateTime.Parse(c.ReleaseDate).Year <= maxYearInt).ToList();
                }

                return movies.OrderByDescending(c => c.Popularity).ToList();
                /*return await _context.Movies
                    .Where(c =>
                        (string.IsNullOrEmpty(searchTerm) || c.Title.ToLower().Contains(searchTerm.ToLower())) &&
                        (maxRuntime == null || c.Runtime <= maxRuntime))
                    .OrderByDescending(c => c.Popularity)
                    .ToListAsync();*/
            }



            return await _context.Movies.ToListAsync();
        }


        public async Task<List<Genre>> getGenre()
        {

            return await _context.Genres.OrderBy(g => g.Name).ToListAsync();
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

        /*public async Task<GenreCounter> GetMaxYear()
        {
            var genreCounts = _context.Movies
                .SelectMany(m => m.Genres) // Flatten all genres across movies
                .GroupBy(g => g.GenreID)   // Group by GenreID
                .Select(g => new GenreCounter
                {
                    GenreID = g.Key,
                    GenreCount = g.Count()
                })
                .ToList();
            return genreCounts
        }*/

        public async Task StoreMovieGenresAsync(int movieId, List<TmdbGenre> genres)
        {
            foreach (var genre in genres)
            {
                var movieGenre = new MovieGenre
                {
                    MovieID = movieId,
                    GenreID = genre.id,
                    Name = genre.name
                };

                _context.MovieGenres.Add(movieGenre);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetMovieCount()
        {
            return await _context.Movies.CountAsync();
        }

        public async Task<List<GenreCounter>> GetGenreMovieCount()
        {
            var result = await _context.MovieGenres
                .GroupBy(mg => mg.GenreID)
                .Select(g => new GenreCounter
                {
                    GenreID = g.Key,
                    GenreCount = g.Select(x => x.MovieID).Distinct().Count()
                })
                .ToListAsync();

            return result;
        }

   

    }
}