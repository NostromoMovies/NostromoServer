// NostromoDBContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nostromo.Server.Utilities;
using System;

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
        public DbSet<Video> Videos { get; set; }
        public DbSet<VideoPlace> VideoPlaces { get; set; }
        public DbSet<DuplicateFile> DuplicateFiles { get; set; }
        public DbSet<ImportFolder> ImportFolders { get; set; }
        public DbSet<CrossRefVideoTMDBMovie> CrossRefVideoTMDBMovies { get; set; }
        public DbSet<TMDBMovieCast> TMDBMovieCasts { get; set; }
        public DbSet<TMDBPerson> TMDBPersons { get; set; }

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

            modelBuilder.Entity<Video>(entity =>
            {
                entity.HasKey(e => e.VideoID);
                entity.Property(e => e.FileName).IsRequired();
                entity.HasMany(e => e.VideoPlaces)
                      .WithOne(vp => vp.Video)
                      .HasForeignKey(vp => vp.VideoID);
            });

            modelBuilder.Entity<VideoPlace>(entity =>
            {
                entity.HasKey(e => e.VideoPlaceID);
                entity.Property(e => e.FilePath).IsRequired();
            });

            modelBuilder.Entity<DuplicateFile>(entity =>
            {
                entity.HasKey(e => e.DuplicateFileID);
                entity.Property(e => e.FilePath1).IsRequired();
                entity.Property(e => e.FilePath2).IsRequired();
            });

            modelBuilder.Entity<ImportFolder>(entity =>
            {
                entity.HasKey(e => e.ImportFolderID);
                entity.Property(e => e.FolderLocation).IsRequired();
            });

            modelBuilder.Entity<CrossRefVideoTMDBMovie>(entity =>
            {
                entity.HasKey(e => e.CrossRefVideoTMDBMovieID);
                entity.HasOne(e => e.Video)
                      .WithMany(v => v.CrossRefVideoTMDBMovies)
                      .HasForeignKey(e => e.VideoID);
                entity.HasOne(e => e.TMDBMovie)
                      .WithMany(m => m.CrossRefVideoTMDBMovies)
                      .HasForeignKey(e => e.TMDBMovieID);
            });

            modelBuilder.Entity<TMDBMovieCast>(entity =>
            {
                entity.HasKey(e => e.TMDBMovieCastID);
                entity.HasOne(e => e.TMDBMovie)
                      .WithMany(m => m.TMDBMovieCasts)
                      .HasForeignKey(e => e.TMDBMovieID);
                entity.HasOne(e => e.TMDBPerson)
                      .WithMany()
                      .HasForeignKey(e => e.TMDBPersonID);
            });

            modelBuilder.Entity<TMDBPerson>(entity =>
            {
                entity.HasKey(e => e.TMDBPersonID);
                entity.Property(e => e.EnglishName).IsRequired();
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
        public virtual ICollection<CrossRefVideoTMDBMovie> CrossRefVideoTMDBMovies { get; set; } = new List<CrossRefVideoTMDBMovie>();
        public virtual ICollection<TMDBMovieCast> TMDBMovieCasts { get; set; } = new List<TMDBMovieCast>();
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

    public class VideoPlace
    {
        public int VideoPlaceID { get; set; }
        public int VideoID { get; set; }
        public string FilePath { get; set; }
        public int ImportFolderID { get; set; }
        public int ImportFolderType { get; set; }
        public virtual Video Video { get; set; }
    }

    public class DuplicateFile
    {
        public int DuplicateFileID { get; set; }
        public string FilePath1 { get; set; }
        public string FilePath2 { get; set; }
        public int ImportFolderID { get; set; }
        public int ImportFolderType { get; set; }
    }

    public class TMDBMovieCast
    {
        public int TMDBMovieCastID { get; set; }
        public int TMDBMovieID { get; set; }
        public int TMDBPersonID { get; set; }
        public int TMDBCreditID { get; set; }
        public string CharacterName { get; set; }
        public int Ordering { get; set; }
        public virtual TMDBMovie TMDBMovie { get; set; }
        public virtual TMDBPerson TMDBPerson { get; set; }
    }

    public class TMDBPerson
    {
        public int TMDBPersonID { get; set; }
        public int TMDBID { get; set; }
        public string EnglishName { get; set; }
        public string EnglishBio { get; set; }
        public string Alias { get; set; }
        public string Gender { get; set; }
        public bool IsRestricted { get; set; }
        public DateTime? BirthDay { get; set; }
        public DateTime? PlaceOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    public class ImportFolder
    {
        public int ImportFolderID { get; set; }
        public int ImportFolderType { get; set; }
        public string FolderLocation { get; set; }
        public bool IsDropSource { get; set; }
        public bool IsDropDestination { get; set; }
        public bool IsWatched { get; set; }
    }

    public class CrossRefVideoTMDBMovie
    {
        public int CrossRefVideoTMDBMovieID { get; set; }
        public int VideoID { get; set; }
        public int TMDBMovieID { get; set; }
        public virtual Video Video { get; set; }
        public virtual TMDBMovie TMDBMovie { get; set; }
    }

    public class Video
    {
        public int VideoID { get; set; }
        public string FileName { get; set; }
        public string CRC32 { get; set; }
        public string SHA1 { get; set; }
        public string MD5 { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public virtual ICollection<VideoPlace> VideoPlaces { get; set; }
        public virtual ICollection<DuplicateFile> DuplicateFiles { get; set; }
        public virtual ICollection<CrossRefVideoTMDBMovie> CrossRefVideoTMDBMovies { get; set; }
    }
}