using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Nostromo.Server.Database
{
    public class NostromoDbContext : DbContext
    {
        public NostromoDbContext(DbContextOptions<NostromoDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TMDBMovie>()
                .HasMany(m => m.Genres)
                .WithMany(g => g.TMDBMovies)
                .UsingEntity<MovieGenre>();
        }
    }

    public class TMDBMovie
    {
        public int MovieID { get; set; }

        public string Title { get; set; }

        public string OriginalTitle { get; set; }

        public string OriginalLanguage { get; set; }

        public string Overview { get; set; }

        public string PosterPath { get; set; }

        public string Status { get; set; }

        public bool Adult { get; set; }

        public ICollection<Genre> Genres { get; set; }
    }

    public class Genre
    {
        public int GenreID { get; set; }
        public string Name { get; set; }

    }

    public class MovieGenre
    {
        public int MovieID { get; set; }
        public int GenreID { get; set; }

        public Movie Movie { get; set; }
        public Genre Genre { get; set; }
    }

    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Salt { get; set; }
    }

    public class SQLite : BaseDatabase<NostromoDbContext>, IDatabase
    {
        private static string _databasePath;

        public SQLite(NostromoDbContext context) : base(context)
        {
        }

        public static string DatabasePath
        {
            get
            {
                if (_databasePath != null)
                    return _databasePath;

                var directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Nostromo",
                    "Database"
                );

                Directory.CreateDirectory(directoryPath);
                _databasePath = Path.Combine(directoryPath, "nostromo.db");
                return _databasePath;
            }
        }

        public override string Name => "SQLite";
        public override int RequiredVersion => 1;

        public override void BackupDatabase(string filename)
        {
            if (File.Exists(DatabasePath))
            {
                File.Copy(DatabasePath, filename, true);
            }
        }

        public override async Task CreateAndUpdateSchema()
        {
            await _context.Database.MigrateAsync();
        }

        public Task CreateAndUpdateSchemaAsync()
        {
            throw new NotImplementedException();
        }

        public override async Task CreateDatabase()
        {
            await _context.Database.EnsureCreatedAsync();
        }

        public Task CreateDatabaseAsync()
        {
            throw new NotImplementedException();
        }

        public override bool DBExists()
        {
            return File.Exists(DatabasePath);
        }

        public override void Init()
        {
            // Any initialization logic
        }

        public override async Task PopulateInitialData()
        {
            if (!_context.Genres.Any())
            {
                var genres = new List<Genre>
                {
                    new Genre { Name = "Action" },
                    new Genre { Name = "Comedy" },
                    new Genre { Name = "Drama" },
                    // Add more genres as needed
                };

                _context.Genres.AddRange(genres);
                await _context.SaveChangesAsync();
            }
        }

        public Task PopulateInitialDataAsync()
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> TestConnection()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        public Task<bool> TestConnectionAsync()
        {
            throw new NotImplementedException();
        }
    }
}