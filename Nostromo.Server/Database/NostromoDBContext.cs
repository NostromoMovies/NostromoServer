// NostromoDBContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Database
{
    public class NostromoDbContext : DbContext
    {
        public NostromoDbContext(DbContextOptions<NostromoDbContext> options) : base(options)
        {
        }

        public DbSet<TMDBMovie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TMDBMovie>(entity =>
            {
                entity.HasKey(e => e.MovieID);
                entity.Property(e => e.Title).IsRequired();
                entity.HasMany(e => e.Genres)
                      .WithMany(e => e.Movies)
                      .UsingEntity<MovieGenre>(
                          j => j.HasOne(mg => mg.Genre)
                                .WithMany()
                                .HasForeignKey(mg => mg.GenreID),
                          j => j.HasOne(mg => mg.Movie)
                                .WithMany()
                                .HasForeignKey(mg => mg.MovieID)
                      );
            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasKey(e => e.GenreID);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
            });
        }
    }

    public class NostromoDbContextFactory : IDesignTimeDbContextFactory<NostromoDbContext>
    {
        // NostromoDbContextFactory
        public NostromoDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NostromoDbContext>();
            var dbDirectory = Path.Combine(Utils.ApplicationPath, "Database");
            var dbPath = Path.Combine(dbDirectory, "nostromo.db");

            Directory.CreateDirectory(dbDirectory);

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            return new NostromoDbContext(optionsBuilder.Options);
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
        public string BackdropPath { get; set; }
        public string ReleaseDate { get; set; }
        public bool Adult { get; set; }
        public decimal Popularity { get; set; }
        public int VoteCount { get; set; }
        public decimal VoteAverage { get; set; }
        public int Runtime { get; set; }
        public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
    }

    public class Genre
    {
        public int GenreID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<TMDBMovie> Movies { get; set; } = new List<TMDBMovie>();
    }

    public class MovieGenre
    {
        public int MovieID { get; set; }
        public int GenreID { get; set; }
        public virtual TMDBMovie Movie { get; set; }
        public virtual Genre Genre { get; set; }
    }

    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Salt { get; set; }
    }
}