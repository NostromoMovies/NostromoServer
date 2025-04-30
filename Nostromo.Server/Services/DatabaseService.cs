using Microsoft.Extensions.Logging;
using Nostromo.Models;
using Nostromo.Server.API.Models;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Nostromo.Server.API.Enums;

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

        Task<(int?, int?, int?)> GetTvShowIdSeasonEpByHashAsync(string hash);
        Task<int?> GetVideoIdByHashAsync(string fileHash);
        Task InsertCrossRefAsync(CrossRefVideoTMDBMovie crossRefModel);

        Task InsertTvCrossRefAsync(CrossRefVideoTvEpisode crossRefModel);
        Task<List<TMDBMovie>> GetFilterMediaGenre(List<int> genresID);
        Task<List<TMDBMovie>> movieRatingsSorted();
        Task<List<Video>> GetAllVideosAsync();
        Task<bool> CheckCrossRefExistsAsync(int videoID, int tmdbMovieID);
        Task StoreMovieCastAsync(int movieId, List<TmdbCastMember> cast);
        Task StoreMovieCrewAsync(int movieId, List<TmdbCrewMember> crew);

        Task StoreTvMediaCastAsync(int mediaId, List<TmdbCastMember> cast);
        Task StoreTvMediaCrewAsync(int mediaId, List<TmdbCrewMember> crew);

        Task<List<TmdbCastMember>> GetCastByMovieIdAsync(int movieId);
        Task<List<TmdbCrewMember>> GetCrewByMovieIdAsync(int movieId);
        Task<List<TmdbCastMember>> GetCastByTvMediaIdAsync(int mediaId);
        Task<List<TmdbCrewMember>> GetCrewByTvMediaIdAsync(int mediaId);
        Task<DateTime> GetCreatedAtByVideoIdAsync(int? videoId);
        Task<Video?> GetVideoByIdAsync(int videoId);
        Task MarkVideoAsUnrecognizedAsync(int? videoId);
        Task MarkVideoAsRecognizedAsync(int? videoId);
        Task<List<Video>> GetAllUnrecognizedVideosAsync();
        Task InsertExampleHashAsync(string ed2kHash, int tmdbId, string title);
        Task StoreTmdbRecommendationsAsync(int movieId, TmdbRecommendation recommendation);
        Task StoreTvRecommendationsAsync(int showId, TvRecommendationResponse recommendation, Dictionary<int, string> GenreDict);
        Task<List<TMDBRecommendation>> GetRecommendationsByMovieIdAsync(int movieId);
        Task<List<TMDBMovie>> GetMoviesByUserAsync(string searchTerm, int maxRuntime, int sortBy, string minYear, string maxYear, List<string> genreIds);
        Task<List<TvShowDto>> GetTvShowsByUserAsync(string searchTerm, int minYear, int maxYear, int sortBy, List<string> genreIds);
        Task<List<Genre>> getGenre();
        Task<int> GetMinYear();
        Task<TvShow> GetTvShowAsync(int id);
        Task StoreMovieGenresAsync(int movieId, List<TmdbGenre> genres);
        Task<int> GetMovieCount();
        Task<List<GenreCounter>> GetGenreMovieCount();
        Task InsertRecommendationGenreAsync(int recommendationId, int genreId, string genreName);
        Task<int?> GetActualRecommendationDbIdAsync(int tmdbMovieId, int recommendationTmdbId);
        Task UpdateMovieCertificationAsync(int movieId, string certification);
        Task UpdateTvCertificationAsync(int showId, string certification);
        
        Task UpdateTvRecommendationCertificationAsync(int showId, string certification);
        Task<Collection> CreateCollectionAsync(string name);
        Task AddItemsToCollectionAsync(int collectionId, List<int>? movieIds, List<int>? tvIds);
        Task<int> GetVideoID(int movieId);
        
        Task<Dictionary<int, string>> GetGenreDictionary();
      
        Task<List<object>> GetAllCollectionsAsync();
        Task UpdateCollectionPosterAsync(int collectionId);
        Task<string> GetCollectionPosterPathAsync(int collectionId);
        Task<List<CollectionItemDto>> GetCollectionItemsAsync(int collectionId);
        Task RemoveItemFromCollectionAsync(int collectionId, int mediaId, string mediaType);
        Task<bool> IsMovieAsync(int mediaId);
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

        public async Task<(int?, int?, int?)> GetTvShowIdSeasonEpByHashAsync(string hash)
        {
            _logger.LogInformation("Searching for TvShowId, SeasonNum and EpisodeNum with hash: {InputHash}", hash);

            var exampleHash = _context.TvExampleHashes.FirstOrDefault(eh => eh.ED2K == hash);
            if (exampleHash != null)
            {
                _logger.LogInformation("Found matching hash: {DatabaseHash} with TvShowID: {ShowID}", exampleHash.ED2K, exampleHash.TvShowId);
            }
            else
            {
                _logger.LogWarning("No matching hash found for: {InputHash}", hash);
            }

            return (exampleHash?.TvShowId, exampleHash?.SeasonNumber, exampleHash?.EpisodeNumber);
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

        public async Task InsertTvCrossRefAsync(CrossRefVideoTvEpisode crossRefModel)
        {
            try
            {
                await _context.CrossRefVideoTvEpisodes.AddAsync(crossRefModel);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully inserted cross-reference for TvEpisodeID={TvEpisodeID} and VideoID={VideoID}",
                    crossRefModel.TvEpisodeId, crossRefModel.VideoID);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while inserting cross-reference for TvEpisodeID={TvEpisodeID} and VideoID={VideoID}",
                    crossRefModel.TvEpisodeId, crossRefModel.VideoID);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while inserting cross-reference for TvEpisodeID={TvEpisodeID} and VideoID={VideoID}",
                    crossRefModel.TvEpisodeId, crossRefModel.VideoID);
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
                    CastId = castMember.cast_id ?? 0,
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

        public async Task StoreTvMediaCastAsync(int mediaId, List<TmdbCastMember> cast)
        {

            var show = await _context.TvShows.FirstOrDefaultAsync(tv => tv.TvShowID == mediaId);
            if (show == null)
            {
                _logger.LogWarning("Show with ID {mediaId} not found in database, skipping cast storage", mediaId);
                return;
            }

            var tmdbIDs = cast.Select(c => c.id).ToList();
            var existingPeopleIds = new HashSet<int>(
                await _context.People
                    .Where(p => tmdbIDs.Contains(p.TMDBID))
                    .Select(p => p.TMDBID)
                    .ToListAsync());

            var showCasts = new List<TvMediaCast>();

            foreach (var castMember in cast)
            {
                TMDBPerson tmdbPerson;

                if (!existingPeopleIds.Contains(castMember.id))
                {
                    tmdbPerson = new TMDBPerson
                    {
                        TMDBID = castMember.id,
                        EnglishName = castMember.name,
                        ProfilePath = castMember.profile_path,
                        CreatedAt = DateTime.UtcNow,
                        LastUpdatedAt = DateTime.UtcNow
                    };
                    try
                    {
                        await _context.People.AddAsync(tmdbPerson);
                        await _context.SaveChangesAsync();
                        existingPeopleIds.Add(castMember.id);
                    }
                    catch (DbUpdateException)
                    {
                        tmdbPerson = await _context.People.FirstOrDefaultAsync(p => p.TMDBID == castMember.id);
                    }

                }
                else
                {
                    tmdbPerson = await _context.People.FirstOrDefaultAsync(p => p.TMDBID == castMember.id);
                }

                var existingCast = await _context.TvMediaCasts
                    .FirstOrDefaultAsync(c => c.MediaID == mediaId &&
                                              c.Id == castMember.id &&
                                              c.KnownForDepartment == castMember.known_for_department);
                if (existingCast == null)
                {
                    var showCast = new TvMediaCast
                    {
                        TMDBPersonID = tmdbPerson.TMDBPersonID,
                        MediaID = show.TvShowID,
                        Adult = castMember.adult,
                        Gender = castMember.gender,
                        Id = castMember.id,
                        KnownForDepartment = castMember.known_for_department,
                        Name = castMember.name,
                        OriginalName = castMember.original_name,
                        Popularity = castMember.popularity,
                        ProfilePath = castMember.profile_path,
                        CastId = castMember.cast_id ?? 0,
                        Character = castMember.character,
                        CreditID = castMember.credit_id,
                        Order = castMember.order
                    };
                    showCasts.Add(showCast);
                }
            }
            await _context.TvMediaCasts.AddRangeAsync(showCasts);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Stored {Count} cast members for show ID {ShowId}", cast.Count, mediaId);
        }

        public async Task StoreTvMediaCrewAsync(int mediaId, List<TmdbCrewMember> crew)
        {
            var show = await _context.TvShows.FirstOrDefaultAsync(tv => tv.TvShowID == mediaId);
            if (show == null)
            {
                _logger.LogWarning("Show with ID {ShowId} not found in database, skipping cast storage", mediaId);
                return;
            }

            var tmdbIds = crew.Select(c => c.id).ToList();
            var existingPeopleIds = new HashSet<int>(
                await _context.People
                    .Where(p => tmdbIds.Contains(p.TMDBID))
                    .Select(p => p.TMDBID)
                    .ToListAsync()
            );

            var showCrews = new List<TvMediaCrew>();

            foreach (var crewMember in crew)
            {
                TMDBPerson tmdbPerson;
                if (!existingPeopleIds.Contains(crewMember.id))
                {
                    tmdbPerson = new TMDBPerson
                    {
                        TMDBID = crewMember.id,
                        EnglishName = crewMember.name,
                        ProfilePath = crewMember.profile_path,
                        CreatedAt = DateTime.UtcNow,
                        LastUpdatedAt = DateTime.UtcNow
                    };

                    try
                    {
                        await _context.People.AddAsync(tmdbPerson);
                        await _context.SaveChangesAsync();
                        existingPeopleIds.Add(crewMember.id);
                    }
                    catch (DbUpdateException)
                    {
                        tmdbPerson = await _context.People.FirstOrDefaultAsync(p => p.TMDBID == crewMember.id);
                    }
                }
                else
                {
                    tmdbPerson = await _context.People.FirstOrDefaultAsync(p => p.TMDBID == crewMember.id);
                }
                var existingCrew = await _context.TvMediaCrews
                    .FirstOrDefaultAsync(c => c.MediaID == show.TvShowID &&
                                              c.Id == crewMember.id &&
                                              c.KnownForDepartment == crewMember.known_for_department);

                if (existingCrew == null)
                {
                    var showCrew = new TvMediaCrew
                    {
                        TMDBPersonID = tmdbPerson.TMDBPersonID,
                        MediaID = show.TvShowID,
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

                    showCrews.Add(showCrew);
                }

            }

            if (showCrews.Any())
            {
                await _context.TvMediaCrews.AddRangeAsync(showCrews);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Stored {Count} cast members for show ID {ShowId}", crew.Count, mediaId);
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

        public async Task<List<TmdbCastMember>> GetCastByTvMediaIdAsync(int mediaId)
        {
            return await _context.TvMediaCasts
                .Where(tmc => tmc.MediaID == mediaId)
                .Select(tmc => new TmdbCastMember
                {
                    id = tmc.Id,
                    name = tmc.Name,
                    original_name = tmc.OriginalName,
                    character = tmc.Character,
                    credit_id = tmc.CreditID,
                    cast_id = tmc.CastId,
                    profile_path = tmc.ProfilePath,
                    popularity = tmc.Popularity,
                    gender = tmc.Gender,
                    known_for_department = tmc.KnownForDepartment,
                    adult = tmc.Adult,
                    order = tmc.Order ?? 0

                }).ToListAsync();
        }

        public async Task<List<TmdbCrewMember>> GetCrewByTvMediaIdAsync(int mediaId)
        {
            return await _context.TvMediaCrews
                .Where(tmc => tmc.MediaID == mediaId)
                .Select(tmc => new TmdbCrewMember
                {
                    id = tmc.Id,
                    name = tmc.Name,
                    original_name = tmc.OriginalName,
                    credit_id = tmc.CreditID,
                    profile_path = tmc.ProfilePath,
                    popularity = tmc.Popularity,
                    gender = tmc.Gender,
                    known_for_department = tmc.KnownForDepartment,
                    department = tmc.Department,
                    job = tmc.Job,
                    adult = tmc.Adult
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
                var existingRecommendation = await _context.Recommendations
                    .FirstOrDefaultAsync(r => r.Id == recommendation.id && r.TMDBMovieID == movieId);

                if (existingRecommendation != null)
                {
                    _logger.LogWarning("TMDB recommendation already exists: {Title} (ID: {Id})", recommendation.title, recommendation.id);
                    return;
                }

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

                await _context.Recommendations.AddAsync(recommendationEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Stored TMDB recommendation: {Title} (ID: {Id})", recommendation.title, recommendation.id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing TMDB recommendation: {Title} (ID: {Id})", recommendation.title, recommendation.id);
                throw;
            }
        }
        
        public async Task StoreTvRecommendationsAsync(int showId, TvRecommendationResponse recommendation, Dictionary<int, string> GenreDict)
        {
            try
            {
                var recommendationEntity = new TvRecommendation
                {
                    Id = recommendation.Id,
                    ShowId = showId,
                    Name = recommendation.Name ?? "Unknown",
                    Adult = recommendation.Adult,
                    BackdropPath = recommendation.backdropPath,
                    OriginalName = recommendation.OriginalName,
                    Overview = recommendation.Overview,
                    PosterPath = recommendation.PosterPath,
                    VoteAverage = recommendation.VoteAverage ?? 0.0,
                    VoteCount = recommendation.VoteCount ?? 0,
                    firstAirDate = recommendation.firstAirDate,
                    Popularity = recommendation.Popularity ?? 0.0,
                    TvRecommendationGenres = recommendation.Genres?
                        .Select(id =>
                        {
                            if (GenreDict.TryGetValue(id, out var genreName) && !string.IsNullOrWhiteSpace(genreName))
                            {
                                return new TvRecommendationGenre
                                {
                                    GenreID = id,
                                    Name = genreName,
                                    //TvRecommendationID = recommendation.Id
                                };
                            }
                            else 
                            {
                                _logger.LogWarning("Genre Id {GenreID} was not found in the database.", id);
                                return null;
                            }
                        }).Where(result => result!= null)
                          .ToList() ?? new List<TvRecommendationGenre>()
                }; 


                var existingRecommendation = await _context.TvRecommendations
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

        public async Task InsertRecommendationGenreAsync(int recommendationId, int genreId, string genreName)
        {
            // Check if the Genre exists
            var existingGenre = await _context.Genres
                .FirstOrDefaultAsync(g => g.GenreID == genreId && g.Name == genreName);

            if (existingGenre == null)
            {
                existingGenre = new Genre
                {
                    GenreID = genreId,
                    Name = genreName
                };

                await _context.Genres.AddAsync(existingGenre);
                await _context.SaveChangesAsync();
            }

            // Check if the RecommendationGenre already exists
            bool alreadyExists = await _context.RecommendationGenres
                .AnyAsync(rg => rg.RecommendationID == recommendationId && rg.GenreID == genreId);

            if (!alreadyExists)
            {
                var recGenre = new RecommendationGenre
                {
                    RecommendationID = recommendationId,
                    GenreID = genreId,
                    Name = genreName
                };

                await _context.RecommendationGenres.AddAsync(recGenre);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int?> GetActualRecommendationDbIdAsync(int tmdbMovieId, int recommendationTmdbId)
        {
            var rec = await _context.Recommendations
                .Where(r => r.TMDBMovieID == tmdbMovieId && r.Id == recommendationTmdbId)
                .Select(r => (int?)r.RecommendationID)
                .FirstOrDefaultAsync();

            return rec;
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
        public async Task<List<TMDBMovie>> GetMoviesByUserAsync(string searchTerm, int maxRuntime, int sortBy, string minYear, string maxYear, List<string> filterGenre)
        {

            IQueryable<TMDBMovie> query = _context.Movies;

            // Apply search term r
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Title.ToLower().Contains(searchTerm.ToLower()));
            }

            // Apply runtime 
            if (maxRuntime > 0)
            {
                query = query.Where(c => c.Runtime <= maxRuntime);
            }

            // Apply genre 
            List<int> genreIds = new List<int>();
            if (filterGenre != null && filterGenre.Any())
            {
                foreach (var genre in filterGenre)
                {
                    if (int.TryParse(genre, out int id))
                    {
                        genreIds.Add(id);
                    }
                }
            }

            // Recently added
            if (sortBy == 3)
            {
                var moviesQuery = _context.CrossRefVideoTMDBMovies
                    .Include(c => c.TMDBMovie)
                    .Where(c => genreIds.Count == 0 || _context.MovieGenres
                        .Where(mg => mg.MovieID == c.TMDBMovie.MovieID && genreIds.Contains(mg.GenreID))
                        .Any())
                    .Select(c => c.TMDBMovie);


                if (!string.IsNullOrEmpty(searchTerm))
                {
                    moviesQuery = moviesQuery.Where(c => c.Title.ToLower().Contains(searchTerm.ToLower()));
                }
                if (maxRuntime > 0)
                {
                    moviesQuery = moviesQuery.Where(c => c.Runtime <= maxRuntime);
                }

                var movies = await moviesQuery
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                // Apply year filters 
                if (!string.IsNullOrEmpty(minYear) && int.TryParse(minYear, out int minYearInt))
                {
                    movies = movies.Where(c => DateTime.TryParse(c.ReleaseDate, out var releaseDate) && releaseDate.Year >= minYearInt).ToList();
                }
                if (!string.IsNullOrEmpty(maxYear) && int.TryParse(maxYear, out int maxYearInt))
                {
                    movies = movies.Where(c => DateTime.TryParse(c.ReleaseDate, out var releaseDate) && releaseDate.Year <= maxYearInt).ToList();
                }

                return movies;
            }
            // For other sort options (alphabetical, highest rated, popularity)
            else
            {
                // Apply genre filter 
                if (genreIds.Any())
                {
                    var movieIdsWithGenres = await _context.MovieGenres
                        .Where(mg => genreIds.Contains(mg.GenreID))
                        .Select(mg => mg.MovieID)
                        .Distinct()
                        .ToListAsync();

                    query = query.Where(m => movieIdsWithGenres.Contains(m.MovieID));
                }

                var movies = await query.ToListAsync();

                // Apply year filters in memory
                if (!string.IsNullOrEmpty(minYear) && int.TryParse(minYear, out int minYearInt))
                {
                    movies = movies.Where(c => DateTime.TryParse(c.ReleaseDate, out var releaseDate) && releaseDate.Year >= minYearInt).ToList();
                }
                if (!string.IsNullOrEmpty(maxYear) && int.TryParse(maxYear, out int maxYearInt))
                {
                    movies = movies.Where(c => DateTime.TryParse(c.ReleaseDate, out var releaseDate) && releaseDate.Year <= maxYearInt).ToList();
                }


                return sortBy switch
                {
                    // Alphabetical
                    1 => movies.OrderBy(c => c.Title.ToLower()).ToList(),
                    // Highest rated
                    2 => movies.OrderByDescending(c => c.VoteAverage).ToList(),
                    // Popularity (default)
                    _ => movies.OrderByDescending(c => c.Popularity).ToList(),
                };
            }
        }

        /*public async Task<List<TMDBMovie>> GetMoviesByUserAsync(string searchTerm, int maxRuntime, int sortBy,string minYear, string maxYear,List<string> filterGenre)
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
                    .ToListAsync();#1#
                
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
                    #1#
                
                
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
                    .ToListAsync();#1#
            }



            return await _context.Movies.ToListAsync();
        }*/

        public async Task<List<TvShowDto>> GetTvShowsByUserAsync(
            string searchTerm, int minYear, int maxYear, int sortBy,List<string> filterGenre)
        {
            List<int> genreIds = new List<int>();
            if (filterGenre != null && filterGenre.Any())
            {
                foreach (var genre in filterGenre)
                {
                    if (int.TryParse(genre, out int id))
                    {
                        genreIds.Add(id);
                    }
                }
            }
            
            var query = _context.TvShows
                .Select(s => new TvShowDto
                {
                    TvShowID = s.TvShowID,
                    OriginalName = s.OriginalName,
                    PosterPath = s.PosterPath,
                    BackdropPath = s.BackdropPath,
                    Overview = s.Overview,
                    FirstAirDate = s.FirstAirDate,
                    Popularity = s.Popularity,
                    VoteAverage = s.VoteAverage,
                    CollectionId = _context.CollectionItems
                        .Where(ci => ci.TmdbTvID == s.TvShowID)
                        .Select(ci => (int?)ci.CollectionID)
                        .FirstOrDefault(),
                    IsInCollection = _context.CollectionItems
                        .Any(ci => ci.TmdbTvID == s.TvShowID)
                });
            
            if (genreIds.Any())
            {
                var tvIdsWithGenres = await _context.TvGenres
                    .Where(mg => genreIds.Contains(mg.GenreID))
                    .Select(mg => mg.TvShowID)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(m => tvIdsWithGenres.Contains(m.TvShowID));
            }

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(s =>
                    s.OriginalName.ToLower().Contains(searchTerm.ToLower()));

            query = sortBy switch
            {
                0 => query.OrderByDescending(c => c.Popularity),
                1 => query.OrderByDescending(c => c.OriginalName.ToLower()),
                2 => query.OrderByDescending(c => c.VoteAverage),
                _ => query.OrderByDescending(c => c.Popularity),
            };

            return await query.ToListAsync();
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
            var genreCounts = await _context.MovieGenres
                .Select(mg => new { mg.GenreID, mg.MovieID })
                .Distinct()
                .GroupBy(x => x.GenreID)
                .ToListAsync();

            var genreList = await _context.Genres.ToListAsync();

            var result = genreCounts
                .Select(g => new GenreCounter
                {

                    GenreName = genreList.FirstOrDefault(genre => genre.GenreID == g.Key)?.Name ?? "Unknown",
                    GenreCount = g.Count()
                })
                .ToList();

            return result;
        }

        public async Task UpdateMovieCertificationAsync(int movieId, string certification)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
                throw new InvalidOperationException($"Movie with ID {movieId} not found.");

            movie.Certification = certification;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTvCertificationAsync(int showId, string certification)
        {
            var show = await _context.TvShows.FindAsync(showId);
            if (show == null)
                throw new InvalidOperationException($"Tv show with ID {showId} not found.");

            show.Certification = certification;
            await _context.SaveChangesAsync();
        }


        public async Task UpdateTvRecommendationCertificationAsync(int showId, string certification)
        {
            var show = await _context.TvRecommendations.FirstOrDefaultAsync(show => show.Id == showId);
            if(show == null)
                throw new InvalidOperationException($"Tv show with ID {showId} not found.");

            show.Certification = certification;
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetVideoID(int movieId)
        {
            int videoId = await _context.CrossRefVideoTMDBMovies
            .Where(movieCrossRef => movieCrossRef.TMDBMovieID == movieId)
            .Select(movieCrossRef => movieCrossRef.VideoID)
            .FirstOrDefaultAsync(); // Returns 0 if not found

            // 2. Check if a valid VideoID was found (assuming 0 is not a valid/linked VideoID)
            if (videoId > 0)
            {
                // Found in the movie table, return it.
                return videoId;
            }
            else
            {
                // 3. Not found in movies (or result was 0), now try finding a VideoID 
                //    by matching the SAME input ID against TvEpisodeId in the episode table.
                videoId = await _context.CrossRefVideoTvEpisodes // Query the episode cross-ref table
                    .Where(episodeCrossRef => episodeCrossRef.TvEpisodeId == movieId) // Match the input against TvEpisodeId
                    .Select(episodeCrossRef => episodeCrossRef.VideoID) // Select the linked VideoID
                    .FirstOrDefaultAsync(); // Returns 0 if not found

                // 4. Return the result from the episode table lookup 
                //    (This will be 0 if it wasn't found in either table)
                return videoId;
            }
        }


        public async Task<Collection> CreateCollectionAsync(string name)
        {
            var collection = new Collection
            {
                Name = name
            };

            _context.Collections.Add(collection);
            await _context.SaveChangesAsync();

            return collection;
        }

        public async Task AddItemsToCollectionAsync(int collectionId, List<int>? movieIds, List<int>? tvIds)
        {
            if (movieIds != null)
            {
                foreach (var movieId in movieIds)
                {
                    var existingMovieItem = await _context.CollectionItems
                        .FirstOrDefaultAsync(ci => ci.CollectionID == collectionId && ci.TmdbMovieID == movieId);

                    if (existingMovieItem == null)
                    {
                        var item = new CollectionItem
                        {
                            CollectionID = collectionId,
                            TmdbMovieID = movieId
                        };
                        _context.CollectionItems.Add(item);

                        var movie = await _context.Movies.FindAsync(movieId);
                        if (movie != null)
                        {
                            movie.IsInCollection = true;
                        }
                    }
                }
            }

            if (tvIds != null)
            {
                foreach (var tvId in tvIds)
                {
                    var existingTvItem = await _context.CollectionItems
                        .FirstOrDefaultAsync(ci => ci.CollectionID == collectionId && ci.TmdbTvID == tvId);

                    if (existingTvItem == null)
                    {
                        var item = new CollectionItem
                        {
                            CollectionID = collectionId,
                            TmdbTvID = tvId
                        };
                        _context.CollectionItems.Add(item);

                        var show = await _context.TvShows.FindAsync(tvId);
                        if (show != null)
                        {
                            show.IsInCollection = true;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveItemFromCollectionAsync(int collectionId, int mediaId, string mediaType)
        {
            CollectionItem item = null;

            if (mediaType == "movie")
            {
                item = await _context.CollectionItems
                    .FirstOrDefaultAsync(ci => ci.CollectionID == collectionId && ci.TmdbMovieID == mediaId);

                if (item != null)
                {
                    var movie = await _context.Movies.FirstOrDefaultAsync(m => m.MovieID == mediaId);
                    if (movie != null)
                        movie.IsInCollection = false;
                }
            }
            else if (mediaType == "tv")
            {
                item = await _context.CollectionItems
                    .FirstOrDefaultAsync(ci => ci.CollectionID == collectionId && ci.TmdbTvID == mediaId);

                if (item != null)
                {
                    var show = await _context.TvShows.FirstOrDefaultAsync(t => t.TvShowID == mediaId);
                    if (show != null)
                        show.IsInCollection = false;
                }
            }

            if (item != null)
            {
                _context.CollectionItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<object>> GetAllCollectionsAsync()
        {
            var collections = await _context.Collections
                .Include(c => c.Items)
                .Select(c => new
                {
                    c.CollectionID,
                    c.Name,
                    c.PosterPath,
                    Items = c.Items.Select(item => new
                    {
                        item.CollectionItemID,
                        item.CollectionID,
                        item.TmdbMovieID,
                        item.TmdbTvID
                    }).ToList()
                })
                .ToListAsync();

            return collections.Cast<object>().ToList();
        }

        public async Task UpdateCollectionPosterAsync(int collectionId)
        {
            var collection = await _context.Collections
                .Include(c => c.Items)
                    .ThenInclude(i => i.TmdbMovie)
                .Include(c => c.Items)
                    .ThenInclude(i => i.TmdbTv)
                .FirstOrDefaultAsync(c => c.CollectionID == collectionId);

            if (collection == null) return;

            var oldestItem = collection.Items
                .Select(item => new
                {
                    PosterPath = item.TmdbMovie?.PosterPath ?? item.TmdbTv?.PosterPath,
                    ReleaseDate = item.TmdbMovie != null
                        ? (DateTime.TryParse(item.TmdbMovie.ReleaseDate, out var date) ? date : (DateTime?)null)
                        : item.TmdbTv?.FirstAirDate
                })
                .Where(x => x.PosterPath != null && x.ReleaseDate != null)
                .OrderBy(x => x.ReleaseDate)
                .FirstOrDefault();

            if (oldestItem != null)
            {
                collection.PosterPath = oldestItem.PosterPath;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GetCollectionPosterPathAsync(int collectionId)
        {
            var collection = await _context.Collections.FindAsync(collectionId);

            return collection?.PosterPath;
        }

        public async Task<List<CollectionItemDto>> GetCollectionItemsAsync(int collectionId)
        {
            var movieItems = await _context.CollectionItems
                .Where(ci => ci.CollectionID == collectionId && ci.TmdbMovieID != null)
                .Select(ci => new CollectionItemDto
                {
                    MovieID = ci.TmdbMovie.MovieID,
                    Title = ci.TmdbMovie.Title,
                    PosterPath = ci.TmdbMovie.PosterPath,
                    MediaType = "movie",
                    CollectionId = ci.CollectionID,
                    IsInCollection = true
                })
                .ToListAsync();

            var tvItems = await _context.CollectionItems
                .Where(ci => ci.CollectionID == collectionId && ci.TmdbTvID != null)
                .Select(ci => new CollectionItemDto
                {
                    TvShowID = ci.TmdbTvID,
                    Title = ci.TmdbTv.OriginalName,
                    PosterPath = ci.TmdbTv.PosterPath,
                    MediaType = "tv",
                    CollectionId = ci.CollectionID,
                    IsInCollection = true
                })
                .ToListAsync();

            var combinedItems = movieItems.Concat(tvItems).ToList();

            return combinedItems;
        }

        public async Task<bool> IsMovieAsync(int mediaId)
        {
            return await _context.Movies.AnyAsync(m => m.TMDBID == mediaId);
        }


        public async Task <Dictionary<int, string>> GetGenreDictionary()
        {
            var GenreDict = await _context.Genres.ToDictionaryAsync(g => g.GenreID, g => g.Name);
            return GenreDict;
        }
    }
}