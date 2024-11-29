using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nostromo.Server.Utilities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Nostromo.Models;

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
        public DbSet<AuthToken> AuthTokens { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<VideoPlace> VideoPlaces { get; set; }
        public DbSet<ImportFolder> ImportFolders { get; set; }
        public DbSet<DuplicateFile> DuplicateFiles { get; set; }
        public DbSet<TMDBMovieCast> MovieCasts { get; set; }
        public DbSet<TMDBPerson> People { get; set; }
        public DbSet<CrossRefVideoTMDBMovie> CrossRefVideoTMDBMovies { get; set; }
        public DbSet<ExampleHash> ExampleHash { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TMDBMovie>(entity =>
            {
                entity.HasKey(e => e.MovieID);
                entity.Property(e => e.MovieID).HasColumnName("TMDBMovieID");
                entity.Property(e => e.TMDBID).IsRequired();
                entity.Property(e => e.TMDBCollectionID);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.OriginalTitle);
                entity.Property(e => e.OriginalLanguage);
                entity.Property(e => e.IsAdult);
                entity.Property(e => e.IsVideo);
                entity.Property(e => e.Overview);
                entity.Property(e => e.Runtime);
                entity.Property(e => e.UserRating).HasColumnType("real");
                entity.Property(e => e.UserVotes);
                entity.Property(e => e.ReleaseDate);
                entity.Property(e => e.PosterPath);
                entity.Property(e => e.BackdropPath);
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.LastUpdatedAt);

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
                entity.Property(e => e.Salt).IsRequired();
                entity.Property(e => e.IsAdmin);
                entity.Property(e => e.CreatedAt);
            });

            modelBuilder.Entity<AuthToken>(entity =>
            {
                entity.HasKey(e => e.AuthId);
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.DeviceName);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId);
            });

            modelBuilder.Entity<Video>(entity =>
            {
                entity.HasKey(e => e.VideoID);
                entity.Property(e => e.FileName).IsRequired();
                entity.Property(e => e.ED2K).IsRequired();
                entity.Property(e => e.CRC32).IsRequired();
                entity.Property(e => e.MD5).IsRequired();
                entity.Property(e => e.SHA1).IsRequired();
                entity.Property(e => e.FileSize);
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedAt);
            });

            modelBuilder.Entity<VideoPlace>(entity =>
            {
                entity.HasKey(e => e.VideoPlaceID);
                entity.Property(e => e.FilePath).IsRequired();
                entity.Property(e => e.ImportFolderID).IsRequired();
                entity.Property(e => e.ImportFolderType).IsRequired();

                entity.HasOne(e => e.Video)
                      .WithMany()
                      .HasForeignKey(e => e.VideoID);
            });

            modelBuilder.Entity<ImportFolder>(entity =>
            {
                entity.HasKey(e => e.ImportFolderID);
                entity.Property(e => e.ImportFolderType);
                entity.Property(e => e.FolderLocation).HasColumnName("ImportFolderLocation");
                entity.Property(e => e.IsDropSource);
                entity.Property(e => e.IsDropDestination);
                entity.Property(e => e.IsWatched);
            });

            modelBuilder.Entity<DuplicateFile>(entity =>
            {
                entity.HasKey(e => e.DuplicateFileID);
                entity.Property(e => e.FilePath1).IsRequired();
                entity.Property(e => e.FilePath2).IsRequired();
                entity.Property(e => e.ImportFolderID);
                entity.Property(e => e.ImportFolderType);
            });

            modelBuilder.Entity<TMDBMovieCast>(entity =>
            {
                entity.HasKey(e => e.TMDBMovieCastID);
                entity.Property(e => e.TMDBPersonID);
                entity.Property(e => e.TMDBCreditID);
                entity.Property(e => e.CharacterName);
                entity.Property(e => e.Ordering);
            });

            modelBuilder.Entity<TMDBPerson>(entity =>
            {
                entity.HasKey(e => e.TMDBPersonID);
                entity.Property(e => e.TMDBID);
                entity.Property(e => e.EnglishName);
                entity.Property(e => e.EnglishBio);
                entity.Property(e => e.Alias).HasColumnName("Aliases");
                entity.Property(e => e.Gender);
                entity.Property(e => e.IsRestricted);
                entity.Property(e => e.BirthDay);
                entity.Property(e => e.PlaceOfBirth);
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.LastUpdatedAt);
            });

            modelBuilder.Entity<CrossRefVideoTMDBMovie>(entity =>
            {
                entity.HasKey(e => e.CrossRefVideoTMDBMovieID);

                entity.HasOne(e => e.Video)
                      .WithMany() // Assuming no navigation property in Video
                      .HasForeignKey(e => e.VideoID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.TMDBMovie)
                      .WithMany() // Assuming no navigation property in TMDBMovie
                      .HasForeignKey(e => e.TMDBMovieID)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<ExampleHash>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TmdbId);
                entity.Property(e => e.Title);
                entity.Property(e => e.ED2K);

                //SEED DATA
                entity.HasData(
                    new ExampleHash {Id = 1, Title = "Alien", TmdbId = 348, ED2K = "5d886780825db91bbc390f10f1b6c95c" }
                );
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
        public TMDBMovie(TmdbMovieResponse apiMovie)
        {
            MovieID = apiMovie.id;
            Title = apiMovie.title;
            Overview = apiMovie.overview;
            OriginalTitle = apiMovie.originalTitle;
            OriginalLanguage = apiMovie.OriginalLanguage;
            IsAdult = apiMovie.adult;
            IsVideo = apiMovie.video;
            Popularity = apiMovie.popularity;
            VoteAverage = apiMovie.voteAverage;
            VoteCount = apiMovie.voteCount;
            Runtime = apiMovie.runtime ?? 0;
            ReleaseDate = apiMovie.releaseDate;
            PosterPath = apiMovie.posterPath;
            BackdropPath = apiMovie.backdropPath;
            CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            LastUpdatedAt = DateTime.UtcNow;
        }

        public TMDBMovie()
        {
        }

        public int MovieID { get; set; }
        public int TMDBID { get; set; }
        public int? TMDBCollectionID { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string OriginalTitle { get; set; }
        public string OriginalLanguage { get; set; }
        public bool IsAdult { get; set; }
        public bool IsVideo { get; set; }
        public float Popularity { get; set; }
        public float VoteAverage { get; set; }
        public int VoteCount { get; set; }
        public int Runtime { get; set; }
        public decimal UserRating { get; set; }
        public int UserVotes { get; set; }
        public string ReleaseDate { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public int CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
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
        public string Salt { get; set; }
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AuthToken
    {
        public int AuthId { get; set; }
        public int UserId { get; set; }
        public string DeviceName { get; set; }
        public string Token { get; set; }
        public virtual User User { get; set; }
    }

    public class Video
    {
        public int VideoID { get; set; }
        public string FileName { get; set; }
        public string ED2K { get; set; }
        public string CRC32 { get; set; }
        public string MD5 { get; set; }
        public string SHA1 { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
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
        public int TMDBPersonID { get; set; }
        public int TMDBCreditID { get; set; }
        public string CharacterName { get; set; }
        public int Ordering { get; set; }
    }

    public class TMDBPerson
    {
        public int TMDBPersonID { get; set; }
        public int TMDBID { get; set; }
        public string EnglishName { get; set; }
        public string EnglishBio { get; set; }
        public string Alias { get; set; }
        public int Gender { get; set; }
        public bool IsRestricted { get; set; }
        public DateTime? BirthDay { get; set; }
        public string PlaceOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    public class ImportFolder
    {
        public int ImportFolderID { get; set; }
        public int ImportFolderType { get; set; }
        public string FolderLocation { get; set; }
        public int IsDropSource { get; set; }
        public int IsDropDestination { get; set; }
        public int IsWatched { get; set; }
    }

    public class CrossRefVideoTMDBMovie
    {
        public int CrossRefVideoTMDBMovieID { get; set; }
        public int VideoID { get; set; }
        public int TMDBMovieID { get; set; }
        public virtual Video Video { get; set; }
        public virtual TMDBMovie TMDBMovie { get; set; }
    }

    public class ExampleHash
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TmdbId { get; set; }

        public string Title { get; set; }

        public string ED2K { get; set; }
    }
}