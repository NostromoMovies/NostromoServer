using Microsoft.Extensions.Logging;
using Nostromo.Models;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Nostromo.Server.Services
{
    public interface IDatabaseService
    {
        Task<TMDBMovie> GetMovieAsync(int id);
        Task InsertMovieAsync(TmdbMovie movie);
        Task InsertGenreAsync(TmdbGenre genre);
        Task<User> FindUserByUsernameAsync(string username);
        Task CreateUserAsync(Users userModel);
        Task<List<TMDBMovie>> SearchMoviesAsync(string title);
        Task<int?> GetMovieIdByHashAsync(string hash);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IUserRepository _userRepository;
        private readonly NostromoDbContext _context;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(
            IMovieRepository movieRepository,
            IUserRepository userRepository,
            NostromoDbContext context,
            ILogger<DatabaseService> logger)
        {
            _movieRepository = movieRepository;
            _userRepository = userRepository;
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




        public async Task<TMDBMovie> GetMovieAsync(int id)
        {
            return await _movieRepository.GetByIdAsync(id);
        }

        public async Task InsertMovieAsync(TmdbMovie movieModel)
        {
            try
            {
                var movie = new TMDBMovie
                {
                    MovieID = movieModel.id,
                    Title = movieModel.title,
                    OriginalTitle = movieModel.originalTitle,
                    OriginalLanguage = movieModel.OriginalLanguage,
                    Overview = movieModel.overview,
                    PosterPath = movieModel.posterPath,
                    BackdropPath = movieModel.backdropPath,
                    ReleaseDate = movieModel.releaseDate,
                    IsAdult = movieModel.adult,
                    Popularity = movieModel.popularity, // Fixed float to decimal conversion
                    VoteCount = movieModel.voteCount,
                    VoteAverage = movieModel.voteAverage, // Fixed float to decimal conversion
                    Runtime = movieModel.runtime ?? 0 // Fixed nullable int conversion with default value
                };

                // Get or create genres
                if (movieModel.genreIds != null && movieModel.genreIds.Any())
                {
                    var genreIds = movieModel.genreIds.Where(id => id != 0).ToList();
                    var genres = await _context.Genres
                        .Where(g => genreIds.Contains(g.GenreID))
                        .ToListAsync();

                    foreach (var genre in genres)
                    {
                        movie.Genres.Add(genre);
                    }
                }

                await _movieRepository.AddAsync(movie);
                _logger.LogInformation("Successfully inserted movie: {Title} (ID: {Id})", movie.Title, movie.MovieID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting movie {Title} (ID: {Id})", movieModel.title, movieModel.id);
                throw;
            }
        }

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

        public async Task<User> FindUserByUsernameAsync(string username)
        {
            return await _userRepository.FindByUsernameAsync(username);
        }

        public async Task CreateUserAsync(Users userModel)
        {
            try
            {
                var user = new User
                {
                    Username = userModel.username,
                    PasswordHash = userModel.passwordHash,
                    Salt = userModel.salt
                };

                await _userRepository.CreateUserAsync(user);
                _logger.LogInformation("Successfully created user: {Username}", user.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", userModel.username);
                throw;
            }
        }

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
    }
}