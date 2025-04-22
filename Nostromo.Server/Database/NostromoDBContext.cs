using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nostromo.Server.Utilities;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Nostromo.Models;
using Nostromo.Server.API.Models;
using Nostromo.Server.Services;

using Nostromo.Server.Services;

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
        public DbSet<TMDBMovieCrew> MovieCrews { get; set; }
        public DbSet<TMDBPerson> People { get; set; }
        public DbSet<CrossRefVideoTMDBMovie> CrossRefVideoTMDBMovies { get; set; }
        public DbSet<ExampleHash> ExampleHash { get; set; }
        public DbSet<TMDBRecommendation> Recommendations { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<RecommendationGenre> RecommendationGenres { get; set; }
        public DbSet<WatchList> WatchLists { get; set; }
        public DbSet<WatchListItem> WatchListItems { get; set; }

        
        public DbSet<TvShow> TvShows { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<Season> Seasons { get; set; } 
        public DbSet<TvRecommendation> TvRecommendations { get; set; }

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
                              .HasForeignKey(mg => new { mg.GenreID, mg.Name }),
                        j => j.HasOne(mg => mg.Movie)
                              .WithMany()
                              .HasForeignKey(mg => mg.MovieID),
                        j =>
                        {
                            j.HasKey(mg => new { mg.MovieID, mg.GenreID, mg.Name });
                        });
            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasKey(g => new { g.GenreID, g.Name });
                entity.HasIndex(g => new { g.GenreID, g.Name }).IsUnique();
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
                entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .ValueGeneratedOnAdd();

                entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .ValueGeneratedOnAddOrUpdate();
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

                entity.Property(e => e.TMDBMovieID)
                        .IsRequired();

                entity.Property(e => e.TMDBPersonID)
                        .IsRequired();

                entity.Property(e => e.CreditID)
                        .HasMaxLength(50)
                        .IsRequired(false);

                entity.Property(e => e.Order)
                        .IsRequired(false);

                entity.HasOne<TMDBPerson>()
                        .WithMany()
                        .HasForeignKey(e => e.TMDBPersonID)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<TMDBMovie>()
                        .WithMany()
                        .HasForeignKey(e => e.TMDBMovieID)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TMDBMovieCrew>(entity =>
            {
                entity.HasKey(e => e.TMDBMovieCrewID);

                entity.Property(e => e.TMDBMovieID)
                        .IsRequired();

                entity.Property(e => e.TMDBPersonID)
                        .IsRequired();

                entity.Property(e => e.CreditID)
                        .HasMaxLength(50)
                        .IsRequired(false);

                entity.HasOne<TMDBPerson>()
                        .WithMany()
                        .HasForeignKey(e => e.TMDBPersonID)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<TMDBMovie>()
                        .WithMany()
                        .HasForeignKey(e => e.TMDBMovieID)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TMDBPerson>(entity =>
            {
                entity.HasKey(e => e.TMDBPersonID);

                entity.Property(e => e.TMDBID)
                        .IsRequired();

                entity.Property(e => e.EnglishName)
                        .HasMaxLength(255)
                        .IsRequired(false);

                entity.Property(e => e.EnglishBio)
                        .HasColumnType("TEXT")
                        .IsRequired(false);

                entity.Property(e => e.Aliases)
                        .HasColumnName("Aliases")
                        .HasColumnType("TEXT")
                        .IsRequired(false);

                entity.Property(e => e.Gender)
                        .IsRequired(false);

                entity.Property(e => e.IsRestricted)
                        .HasDefaultValue(false)
                        .IsRequired(false);

                entity.Property(e => e.BirthDay)
                        .HasMaxLength(10)
                        .IsRequired(false);

                entity.Property(e => e.PlaceOfBirth)
                        .HasMaxLength(255)
                        .IsRequired(false);

                entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .ValueGeneratedOnAdd();

                entity.Property(e => e.LastUpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .ValueGeneratedOnAddOrUpdate();
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

                // SEED DATA
                entity.HasData(
                    new ExampleHash
                    {
                        Id = 1,
                        Title = "Alien",
                        TmdbId = 348,
                        ED2K = "5d886780825db91bbc390f10f1b6c95c"
                    },
                    new ExampleHash
                    {
                        Id = 2,
                        Title = "Aliens",
                        TmdbId = 679,
                        ED2K = "ee4a746481ec4a6a909943562aefe86a"
                    },
                    new ExampleHash
                    {
                        Id = 3,
                        Title = "Alien 3",
                        TmdbId = 8077,
                        ED2K = "b33d9c30eb480eca99e82dbbab3aad0e"
                    }
                );
            });

            modelBuilder.Entity<TMDBRecommendation>(entity =>
            {
                entity.HasKey(e => e.RecommendationID);

                entity.Property(e => e.RecommendationID).ValueGeneratedOnAdd();
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.TMDBMovieID).IsRequired();
                entity.Property(e => e.Title).IsRequired();

                entity.HasIndex(e => new { e.TMDBMovieID, e.Id }).IsUnique(false);

                entity.HasOne(e => e.Movie)
                      .WithMany()
                      .HasForeignKey(e => e.TMDBMovieID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(r => r.Genres)
                      .WithMany()
                      .UsingEntity<RecommendationGenre>(
                          j => j.HasOne(rg => rg.Genre)
                                .WithMany()
                                .HasForeignKey(rg => new { rg.GenreID, rg.Name }),
                          j => j.HasOne(rg => rg.Recommendation)
                                .WithMany()
                                .HasForeignKey(rg => rg.RecommendationID),
                          j =>
                          {
                              j.HasKey(rg => new { rg.RecommendationID, rg.GenreID, rg.Name });
                          });
            });

            modelBuilder.Entity<WatchList>(entity =>
            {
                entity.HasKey(wl => wl.WatchListID);

                entity.HasOne(wl => wl.User)
                    .WithMany()
                    .HasForeignKey(wl => wl.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WatchListItem>(entity =>
            {
                entity.HasKey(wli => new { wli.WatchListID, wli.MovieID });

                entity.HasOne(wli => wli.WatchList)
                    .WithMany(wl => wl.Items)
                    .HasForeignKey(wli => wli.WatchListID);

                entity.HasOne(wli => wli.Movie)
                    .WithMany()
                    .HasForeignKey(wli => wli.MovieID);
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
        public string? Certification { get; set; }
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
        public string Name { get; set; }

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
        public bool IsRecognized { get; set; } = true;
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
        public bool Adult { get; set; }
        public int Gender { get; set; }
        public int Id { get; set; }
        public string? KnownForDepartment { get; set; }
        public string? Name { get; set; }
        public string? OriginalName { get; set; }
        public double Popularity { get; set; }
        public string? ProfilePath { get; set; }
        public int CastId { get; set; }
        public string? Character { get; set; }
        public string? CreditID { get; set; }
        public int? Order { get; set; }
    }

    public class TMDBMovieCrew
    {
        public int TMDBMovieCrewID { get; set; }
        public int TMDBMovieID { get; set; }
        public int TMDBPersonID { get; set; }
        public bool Adult { get; set; }
        public int Gender { get; set; }
        public int Id { get; set; }
        public string? KnownForDepartment { get; set; }
        public string? Name { get; set; }
        public string? OriginalName { get; set; }
        public double Popularity { get; set; }
        public string? ProfilePath { get; set; }
        public string? CreditID { get; set; }
        public string? Department { get; set; }
        public string? Job { get; set; }
    }

    public class TMDBPerson
    {
        public int TMDBPersonID { get; set; }

        public int TMDBID { get; set; }
        public string? EnglishName { get; set; }
        public string? Aliases { get; set; }
        public string? EnglishBio { get; set; }
        public int? Gender { get; set; }
        public bool? IsRestricted { get; set; }
        public string? BirthDay { get; set; }
        public string? DeathDay { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? ProfilePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
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
        public DateTime CreatedAt { get; set; }
        public virtual Video Video { get; set; }
        public virtual TMDBMovie TMDBMovie { get; set; }
    }

    public class ExampleHash
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; }
        public string ED2K { get; set; }
    }

    public class TMDBRecommendation
    {
        public int RecommendationID { get; set; }
        public int Id { get; set; }
        public int TMDBMovieID { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public string MediaType { get; set; }
        public bool Adult { get; set; }
        public string OriginalLanguage { get; set; }
        public double Popularity { get; set; }
        public string ReleaseDate { get; set; }
        public bool Video { get; set; }
        public double VoteAverage { get; set; }
        public int VoteCount { get; set; }

        public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
        public virtual TMDBMovie Movie { get; set; }
    }

    public class RecommendationGenre
    {
        public int RecommendationID { get; set; }
        public int GenreID { get; set; }
        public string Name { get; set; }

        public virtual TMDBRecommendation Recommendation { get; set; }
        public virtual Genre Genre { get; set; }
    }

    public class WatchList
    {
        public int WatchListID { get; set; }
        public string Name { get; set; }

        public int UserID { get; set; }
        public virtual User? User { get; set; }

        public virtual ICollection<WatchListItem> Items { get; set; } = new List<WatchListItem>();
    }

    public class WatchListItem
    {
        public int WatchListID { get; set; }
        public virtual WatchList WatchList { get; set; }

        public int MovieID { get; set; }
        public virtual TMDBMovie Movie { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
    
    public class TvShow
     {
         public TvShow(TmdbTvResponse api)
         {
             TvShowID = api.Id;
             Seasons = api.Seasons?.ConvertAll(season => new Season(season, TvShowID)) ?? new List<Season>();
             Adult = api.Adult;
             OriginalLanguage = api.OriginalLanguage;
             OriginalName = api.OriginalName;
             Overview = api.Overview ?? "";
             Popularity = api.Popularity ?? 0.0;
             PosterPath = api.PosterPath;
             BackdropPath = api.BackdropPath;
             VoteAverage = api.VoteAverage ?? 0.0;
             VoteCount = api.VoteCount ?? 0;
         }

         [Key]
         public int TvShowID { get; set; }
         
         public bool Adult { get; set; }
         
         public string OriginalLanguage { get; set; }
         
         public string OriginalName { get; set; }
         
         public string Overview { get; set; }
         
         public double Popularity { get; set; }
         
         public string? PosterPath { get; set; }
         
         public string? BackdropPath { get; set; }
         
         public string? FirstAirDate { get; set; }
         
         public double VoteAverage { get; set; }

         public int VoteCount { get; set; }
         
         public List<Season> Seasons { get; set; }
         
         public TvShow(){
         }
         
     }

     public class Season
     {
         public Season(TmdbTvSeasonResponse api, int tvShowId)
         {
             SeasonID = api.SeasonID;
             TvShowID = tvShowId;
             seasonName = api.seasonName;
             SeasonNumber = api.SeasonNumber;
             Airdate = api.Airdate;
             EpisodeCount = api.EpisodeCount;
             Overview = api.Overview;
             PosterPath = api.PosterPath;
             VoteAverage = api.VoteAverage;
         }
         
         public Season(){}
         [Key]
         public int SeasonID { get; set; }
         
         [ForeignKey("TvShow")]
         public int TvShowID { get; set; }
         
         public string seasonName { get; set; }
         
         public virtual TvShow TvShow { get; set; }
         
         public int SeasonNumber { get; set; }
         
         public string Airdate { get; set; }
         
         public int EpisodeCount { get; set; }
         
         public string Overview { get; set; }
         
         public string PosterPath { get; set; }
         
         public double VoteAverage { get; set; }
         
         //public virtual ICollection<Episode> Episodes { get; set; } = new List<Episode>();
     }

     public class Episode
     {
         public Episode(TmdbTvEpisodeResponse api, int seasonID, int episodeNum)
         {
             EpisodeID = api.EpisodeID;
             SeasonID = seasonID;
             EpisodeNumber = episodeNum;
             EpisodeName = api.EpisodeName;
             Airdate = api.Airdate;
             Overview = api.Overview;
             SeasonNumber = api.SeasonNumber;
             Runtime = api.Runtime;
             VoteAverage = api.VoteAverage;
             VoteCount = api.VoteCount;
             StillPath = api.StillPath;
         }

         public Episode()
         {

         }

         [Key] public int EpisodeID { get; set; }

         [ForeignKey("Season")] public int SeasonID { get; set; }
         public virtual Season Season { get; set; }

         public string EpisodeName { get; set; }

         public int EpisodeNumber { get; set; }

         public string Airdate { get; set; }

         public string Overview { get; set; }

         public int SeasonNumber { get; set; }

         public int Runtime { get; set; }

         public double VoteAverage { get; set; }

         public int VoteCount { get; set; }

         public string StillPath { get; set; }

     }
     public class TvRecommendation
     {
         
         [Key]
         [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
         public int RecommendationID { get; set; }

         [Required]
         public int Id { get; set; }

         [Required]
         public int ShowId { get; set; }

         [Required]
         public string Name { get; set; }
         
         public bool Adult { get; set; }
         
         public string BackdropPath { get; set; }
         
         public string OriginalName { get; set; }
         
         public string Overview { get; set; }
         
         public string PosterPath { get; set; }
         
         public string MediaType { get; set; }
         
         public List<Genre> Genres { get; set; }
         
         public double Popularity { get; set; }
         
         public string firstAirDate { get; set; }
         
         public double VoteAverage { get; set; }
         
         public int VoteCount { get; set; }
         
         public virtual TvShow TvShow { get; set; }
         
     }
}
