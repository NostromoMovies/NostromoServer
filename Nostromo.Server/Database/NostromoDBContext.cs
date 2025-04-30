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
        public DbSet<Collection> Collections { get; set; }
        public DbSet<CollectionItem> CollectionItems { get; set; }
        public DbSet<WatchStatistic> WatchStatistics { get; set; }

        public DbSet<TvGenre> TvGenres { get; set; } 
        public DbSet<TvMediaCast> TvMediaCasts { get; set; }  
        public DbSet<TvMediaCrew> TvMediaCrews { get; set; }  
        public DbSet<TvRecommendationGenre> TvRecommendationGenres { get; set; } 
        public DbSet<TvExampleHash> TvExampleHashes { get; set; }
        
        public DbSet<CrossRefVideoTvEpisode> CrossRefVideoTvEpisodes { get; set; }
        
        public DbSet<Profile> Profiles { get; set; }

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
                entity.Property(e => e.IsInCollection).HasDefaultValue(false);

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
                
                entity.HasIndex(e => e.TMDBID)
                    .IsUnique();
                
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
                      .WithMany()
                      .HasForeignKey(e => e.VideoID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.TMDBMovie)
                      .WithMany()
                      .HasForeignKey(e => e.TMDBMovieID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CrossRefVideoTvEpisode>(entity =>
            {
                entity.HasKey(e => e.CrossRefVideoTvEpisodeID);

                entity.HasOne(e => e.Video)
                    .WithMany()
                    .HasForeignKey(e => e.VideoID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.TvEpisode)
                    .WithMany()
                    .HasForeignKey(e => e.TvEpisodeId)
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
                        ED2K = "da1a506c0ee1fe6c46ec64fd57faa924"
                    },
                    new ExampleHash
                    {
                        Id = 3,
                        Title = "Alien 3",
                        TmdbId = 8077,
                        ED2K = "b33d9c30eb480eca99e82dbbab3aad0e"
                    },
                    new ExampleHash
                    {
                        Id = 4,
                        Title = "2001: A Space Odyssey",
                        TmdbId = 62,
                        ED2K = "b8b18d2129c23ce7be0f20192ab5cc7d"
                    },
                    new ExampleHash
                    {
                        Id = 5,
                        Title = "Blade Runner",
                        TmdbId = 78,
                        ED2K = "f1f92c24015ee61c26ee14e1a620c2f1"
                    },
                    new ExampleHash
                    {
                        Id = 6,
                        Title = "Close Encounters of the Third Kind",
                        TmdbId = 840,
                        ED2K = "5c397afacb0a7e4ca53a208c70a60312"
                    },
                    new ExampleHash
                    {
                        Id = 7,
                        Title = "Big Hero 6",
                        TmdbId = 177572,
                        ED2K = "bd48b94f65b1cb6526acc0cd0b52e733"
                    },
                    new ExampleHash
                    {
                        Id = 8,
                        Title = "Arrival",
                        TmdbId = 329865,
                        ED2K = "2bc38b9668690f1b5d83bcd5d0b8875c"
                    },
                    new ExampleHash
                    {
                        Id = 9,
                        Title = "A.I. Artificial Intelligence",
                        TmdbId = 644,
                        ED2K = "aa9f497e20846c5018f47e025d06d190"
                    },
                    new ExampleHash
                    {
                        Id = 10,
                        Title = "Blade Runner 2049",
                        TmdbId = 335984,
                        ED2K = "6dc784b7b42faa32d70106b6008137fc"
                    },
                    new ExampleHash
                    {
                        Id = 11,
                        Title = "Flight of the Navigator",
                        TmdbId = 10122,
                        ED2K = "8891dba5d7423e41be1670f5022514a6"
                    },
                    new ExampleHash
                    {
                        Id = 12,
                        Title = "The Iron Giant",
                        TmdbId = 10386,
                        ED2K = "77ec11a8b08ee689cb4e8e9cbae406fb"
                    },
                    new ExampleHash
                    {
                        Id = 13,
                        Title = "Meet The Robinsons",
                        TmdbId = 1267,
                        ED2K = "cde961b6799ad092ffe00e17ebd95cdb"
                    },
                    new ExampleHash
                    {
                        Id = 14,
                        Title = "Event Horizon",
                        TmdbId = 8413,
                        ED2K = "e16a3334eaa4a1b36c7ffb0eb2ec0c35"
                    },
                    new ExampleHash
                    {
                        Id = 15,
                        Title = "Lilo & Stitch",
                        TmdbId = 11544,
                        ED2K = "89d725b0be5df4643edcaca155ecf165"
                    },
                    new ExampleHash
                    {
                        Id = 16,
                        Title = "E.T. The Extra Terrestrial",
                        TmdbId = 601,
                        ED2K = "4ca3e7ad70bd6595ee68fabfd0273534"
                    },
                    new ExampleHash
                    {
                        Id = 17,
                        Title = "The Thing",
                        TmdbId = 1091,
                        ED2K = "a60bc42199d8a34638087b267bea1400"
                    },
                    new ExampleHash
                    {
                        Id = 18,
                        Title = "The Last Starfighter",
                        TmdbId = 11884,
                        ED2K = "f69fa1b76e69c8141e52945175bd81d0"
                    },
                    new ExampleHash
                    {
                        Id = 19,
                        Title = "Treasure Planet",
                        TmdbId = 9016,
                        ED2K = "b092919efab8f3c27e5e67cf15a02acd"
                    },
                    new ExampleHash
                    {
                        Id = 20,
                        Title = "WALL-E",
                        TmdbId = 10681,
                        ED2K = "8ca300a5aa1a73c8419f4d1622c3364d"
                    },
                    new ExampleHash
                    {
                        Id = 21,
                        Title = "Total Recall",
                        TmdbId = 861,
                        ED2K = "c0c717a4f8fad3366520d47c702ab5ad"
                    },
                    new ExampleHash
                    {
                        Id = 22,
                        Title = "Altered States",
                        TmdbId = 11542,
                        ED2K = "d68b4df74882553d70ddf8b1bfa4c510"
                    },
                    new ExampleHash
                    {
                    Id = 23,
                    Title = "Con Air",
                    TmdbId = 1701,
                    ED2K = "da7b55d4b775b92635a34b10ef30ec88"
                    },
                    new ExampleHash
                    {
                    Id = 24,
                    Title = "Dracula's Daughter",
                    TmdbId = 22440,
                    ED2K = "e0dbd16993d8305df99162d76d196942"
                    },
                    new ExampleHash
                    {
                    Id = 25,
                    Title = "Drunken Master",
                    TmdbId = 11230,
                    ED2K = "d66e8ed433fbadb4dd7a2563e07e5133"
                    },
                    new ExampleHash
                    {
                    Id = 26,
                    Title = "I Stand Alone",
                    TmdbId = 1567,
                    ED2K = "07321dbd990d80af952132132f3aaba7"
                    },
                    new ExampleHash
                    {
                    Id = 27,
                    Title = "Mad God",
                    TmdbId = 846867,
                    ED2K = "a41745e62f6b2f0c9908ce278e817e77"
                    },
                    new ExampleHash
                    {
                    Id = 28,
                    Title = "Journey To The Center Of The Earth",
                    TmdbId = 88751,
                    ED2K = "bb8dcf1c11c68c414631bf1b2e861a25"
                    },
                    new ExampleHash
                    {
                    Id = 29,
                    Title = "Paprika",
                    TmdbId = 4977,
                    ED2K = "8dac63001e29e8e9410f1d22ccd5eedc"
                    },
                    new ExampleHash
                    {
                    Id = 30,
                    Title = "Q - The Winged Serpent",
                    TmdbId = 29780,
                    ED2K = "50cb41f65ff732f40d1f52e7758cff15"
                    },
                    new ExampleHash
                    {
                    Id = 31,
                    Title = "Little Big Man",
                    TmdbId = 11040,
                    ED2K = "b11eabf34f194c2429247bd699918d73"
                    },
                    new ExampleHash
                    {
                    Id = 32,
                    Title = "Harakiri",
                    TmdbId = 14537,
                    ED2K = "14e1ec9339fcc1a2b0754a288acee7ce"
                    },
                    new ExampleHash
                    {
                    Id = 33,
                    Title = "Rons Gone Wrong",
                    TmdbId = 482321,
                    ED2K = "bf80cbaadf242c4043d44385de75638d"
                    },
                    new ExampleHash
                    {
                    Id = 34,
                    Title = "Strangers on a Train",
                    TmdbId = 845,
                    ED2K = "a915eb32bff0604eec9afc47806b7a09"
                    },
                    new ExampleHash
                    {
                    Id = 35,
                    Title = "Rio Bravo",
                    TmdbId = 301,
                    ED2K = "bbd17e2e0e39f177fb42ba12fbc3a397"
                    },
                    new ExampleHash
                    {
                    Id = 36,
                    Title = "The Omega Man",
                    TmdbId = 1450,
                    ED2K = "ff4864650ec980159573479b67151859"
                    },
                    new ExampleHash
                    {
                    Id = 37,
                    Title = "The Pink Panther",
                    TmdbId = 936,
                    ED2K = "53b3060dde08445467d73e8e07976f83"
                    },
                    new ExampleHash
                    {
                    Id = 38,
                    Title = "Tremors",
                    TmdbId = 9362,
                    ED2K = "47c1596a0d6c657cc0f030593156eb9d"
                    },
                    new ExampleHash
                    {
                    Id = 39,
                    Title = "The Skin I Live In",
                    TmdbId = 63311,
                    ED2K = "7b76844584faec9599909bf10113d9cf"
                    },
                    new ExampleHash
                    {
                    Id = 40,
                    Title = "The Talented Mr. Ripley",
                    TmdbId = 1213,
                    ED2K = "10d831b6f8db5f6eaaa35d485d9b38e5"
                    },
                    new ExampleHash
                    {
                    Id = 41,
                    Title = "Young Frankenstein",
                    TmdbId = 3034,
                    ED2K = "548e716ce84608f6aabe0aaeb23ad855"
                    }
                );
            });

            modelBuilder.Entity<TvExampleHash>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Title);
                entity.Property(e => e.TvShowId);
                entity.Property(e => e.SeasonNumber);
                entity.Property(e => e.EpisodeNumber);
                entity.Property(e => e.ED2K);

                entity.HasData(
                    new TvExampleHash
                    {
                        Id = 1,
                        Title = "The Blacklist",
                        TvShowId = 46952,
                        SeasonNumber = 1,
                        EpisodeNumber = 1,
                        ED2K = "a413da8e3e3bb02237795b2dc9e06b8d"
                    },
                    new TvExampleHash
                    {
                        Id = 2,
                        Title = "The Blacklist",
                        TvShowId = 46952,
                        SeasonNumber = 1,
                        EpisodeNumber = 2,
                        ED2K = "ee4a746481ec4a6a909943562aefe86a"
                    },
                    new TvExampleHash
                    {
                        Id = 3,
                        Title = "The Blacklist",
                        TvShowId = 46952,
                        SeasonNumber = 1,
                        EpisodeNumber = 3,
                        ED2K = "a73c8cf075a960af6004a257432b2435"
                    },
                    new TvExampleHash
                    {
                        Id = 4,
                        Title = "The Blacklist",
                        TvShowId = 46952,
                        SeasonNumber = 1,
                        EpisodeNumber = 4,
                        ED2K = "ea85563f8f9c051cab70a0139c5118da"
                    },
                    new TvExampleHash
                    {
                        Id = 5,
                        Title = "The Blacklist",
                        TvShowId = 46952,
                        SeasonNumber = 1,
                        EpisodeNumber = 5,
                        ED2K = "c5f51c3dc5b4b45c68e428ccc062949f"
                    },
                    new TvExampleHash
                    {
                        Id = 6,
                        Title = "The Blacklist",
                        TvShowId = 46952,
                        SeasonNumber = 2,
                        EpisodeNumber = 1,
                        ED2K = "7203ced34b4989a4527457a4c564e2c1"
                    },
                    new TvExampleHash
                    {
                        Id = 7,
                        Title = "The Blacklist",
                        TvShowId = 46952,
                        SeasonNumber = 2,
                        EpisodeNumber = 2,
                        ED2K = "8accb9f07416005acdd4d4d9bc790295"
                    },
                    new TvExampleHash
                    {
                        Id = 8,
                        Title = "The Blacklist",
                        TvShowId = 46952,
                        SeasonNumber = 2,
                        EpisodeNumber = 3,
                        ED2K = "41da21faa145d66664535b5084240096"
                    },
                    new TvExampleHash
                    {
                        Id = 9,
                        Title = "The Blacklist",
                        TvShowId = 46952,
                        SeasonNumber = 2,
                        EpisodeNumber = 4,
                        ED2K = "2bda47a34c226363615c0355e001683b"
                    },
                    new TvExampleHash
                    {
                        Id = 10,
                        Title = "The National Anthem",
                        TvShowId = 42009,
                        SeasonNumber = 1,
                        EpisodeNumber = 1,
                        ED2K = "15f73bad52cd5ce13a95673e90708939"
                    },
                    new TvExampleHash
                    {
                        Id = 11,
                        Title = "Fifteen Million Merits",
                        TvShowId = 42009,
                        SeasonNumber = 1,
                        EpisodeNumber = 2,
                        ED2K = "1f0103e25e21ae6b3092a3a53c91f21b"
                    },
                    new TvExampleHash
                    {
                        Id = 12,
                        Title = "The Entire History of You",
                        TvShowId = 42009,
                        SeasonNumber = 1,
                        EpisodeNumber = 3,
                        ED2K = "4dc74beecc6eb8937b540ff4a51a8bea"
                    }
                );
            });
            

            modelBuilder.Entity<TvRecommendation>(entity =>
            {
                entity.HasOne(e => e.TvShow)
                    .WithMany()
                    .HasForeignKey(e => e.ShowId)
                    .HasPrincipalKey(tv => tv.TvShowID);
                
                entity.HasMany(r => r.TvRecommendationGenres)
                    .WithOne(r => r.TvRecommendation)
                    .HasForeignKey(r => r.TvRecommendationID);
            });

            modelBuilder.Entity<TvRecommendationGenre>(entity =>
            {
                entity.HasKey(r => new
                {
                    r.TvRecommendationID,
                    r.GenreID,
                    r.Name
                });
                
                entity.HasOne(r=>r.TvRecommendation)
                    .WithMany(r=>r.TvRecommendationGenres)
                    .HasForeignKey(r=>r.TvRecommendationID);
                
                entity.HasOne(r => r.Genre)
                    .WithMany()
                    .HasForeignKey(r => new { r.GenreID, r.Name })
                    .HasPrincipalKey((g => new { g.GenreID, g.Name }));
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
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(wl => wl.Profile)
                        .WithMany()
                        .HasForeignKey(wl => wl.ProfileID)
                        .OnDelete(DeleteBehavior.SetNull);

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

            modelBuilder.Entity<TvShow>(entity =>
            {
                entity.HasKey(e => e.TvShowID);
                entity.Property(e => e.OriginalLanguage);
                entity.Property(e => e.OriginalName);
                entity.Property(e => e.Overview);
                entity.Property(e => e.Popularity);
                entity.Property(e => e.PosterPath);
                entity.Property(e => e.BackdropPath);
                entity.Property(e => e.FirstAirDate);
                entity.Property(e => e.VoteAverage);
                entity.Property(e => e.VoteCount);
                entity.Property(e => e.IsInCollection).HasDefaultValue(false);

                entity.HasMany(e => e.Seasons)
                      .WithOne(s => s.TvShow)
                      .HasForeignKey(s => s.TvShowID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(tv => tv.Genres)
                    .WithMany(g => g.TvShows)
                    .UsingEntity<TvGenre>(
                        j => j.HasOne(g => g.Genre)
                            .WithMany()
                            .HasForeignKey(g => new { g.GenreID, g.Name }),

                        j => j.HasOne(g => g.TvShow)
                            .WithMany(tv => tv.TvGenres)
                            .HasForeignKey(g => g.TvShowID),

                        j =>
                        {
                            j.HasKey(g => new { g.TvShowID, g.GenreID, g.Name });
                        }
                    );
            });

            modelBuilder.Entity<TvGenre>(entity =>
            {
                entity.HasKey(g => new { g.TvShowID, g.GenreID, g.Name });

                entity.HasOne(g => g.TvShow)
                    .WithMany(t => t.TvGenres)
                    .HasForeignKey(g => g.TvShowID);

                entity.HasOne(g => g.Genre)
                    .WithMany()
                    .HasForeignKey(g => new { g.GenreID, g.Name })
                    .HasPrincipalKey(g => new { g.GenreID, g.Name });

            });
            
            modelBuilder.Entity<Season>(entity =>
            {
                entity.HasKey(e => e.SeasonID);
                entity.Property(e => e.seasonName);
                entity.Property(e => e.SeasonNumber);
                entity.Property(e => e.Airdate);
                entity.Property(e => e.EpisodeCount);
                entity.Property(e => e.Overview);
                entity.Property(e => e.PosterPath);
                entity.Property(e => e.VoteAverage);

                entity.HasOne(e => e.TvShow)
                      .WithMany(t => t.Seasons)
                      .HasForeignKey(e => e.TvShowID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Episode>(entity =>
            {
                entity.HasKey(e => e.EpisodeID);
                entity.Property(e => e.EpisodeName);
                entity.Property(e => e.EpisodeNumber);
                entity.Property(e => e.Airdate);
                entity.Property(e => e.Overview);
                entity.Property(e => e.SeasonNumber);
                entity.Property(e => e.Runtime);
                entity.Property(e => e.VoteAverage);
                entity.Property(e => e.VoteCount);
                entity.Property(e => e.StillPath);

                entity.HasOne(e => e.Season)
                      .WithMany()
                      .HasForeignKey(e => e.SeasonID)
                      .OnDelete(DeleteBehavior.Cascade);
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
                entity.HasKey(wli => wli.WatchListItemID);

                entity.HasOne(wli => wli.WatchList)
                    .WithMany(wl => wl.Items)
                    .HasForeignKey(wli => wli.WatchListID);

                entity.HasOne(wli => wli.Movie)
                    .WithMany()
                    .HasForeignKey(wli => wli.MovieID)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(wli => wli.TvShow)
                    .WithMany()
                    .HasForeignKey(wli => wli.TvShowID)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasCheckConstraint("CK_WatchListItem_MovieOrTvShow",
                    "(MovieID IS NOT NULL AND TvShowID IS NULL) OR (MovieID IS NULL AND TvShowID IS NOT NULL)");
            });

            modelBuilder.Entity<Collection>(entity =>
            {
                entity.HasKey(c => c.CollectionID);
                entity.Property(c => c.Name).IsRequired();
                entity.Property(c => c.PosterPath);
                entity.HasOne(c => c.Profile)
                    .WithMany()
                    .HasForeignKey(c => c.ProfileID)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CollectionItem>(entity =>
            {
                entity.HasKey(ci => ci.CollectionItemID);

                entity.HasOne(ci => ci.Collection)
                      .WithMany(c => c.Items)
                      .HasForeignKey(ci => ci.CollectionID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.TmdbMovie)
                      .WithMany()
                      .HasForeignKey(ci => ci.TmdbMovieID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.TmdbTv)
                      .WithMany()
                      .HasForeignKey(ci => ci.TmdbTvID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WatchStatistic>(entity =>
            {
                entity.HasKey(ws => ws.MovieID);
                entity.Property(ws => ws.MovieID)
                      .IsRequired();
                entity.Property(ws => ws.WatchDuration)
                      .IsRequired();
                entity.Property(ws => ws.WatchDuration)
                      .HasColumnType("int");
            });
            
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(e => e.ProfileID);

                entity.Property(e => e.ProfileID)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Age)
                    .IsRequired();

                entity.Property(e => e.Adult)
                    .IsRequired();

                entity.Property(e => e.posterPath)
                    .HasMaxLength(300); 
                
                entity.Property(e => e.UserId)
                    .IsRequired();
                
                entity.HasOne(p => p.User)
                    .WithMany(u => u.Profiles)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
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
        public bool IsInCollection { get; set; } = false;
        public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
    }

    public class Genre
    {
        public int GenreID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<TMDBMovie> Movies { get; set; } = new List<TMDBMovie>();
        public virtual ICollection<TvRecommendation> TvRecommendations { get; set; } = new List<TvRecommendation>();
        public virtual ICollection<TvShow> TvShows { get; set; } = new List<TvShow>();
    }

    public class TvGenre
    {
        public int TvShowID { get; set; }
        public int GenreID { get; set; }
        public string Name { get; set; }
        
        public virtual TvShow TvShow { get; set; }
        public virtual Genre Genre { get; set; }
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
        
        public virtual ICollection<Profile> Profiles { get; set; } = new List<Profile>();

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
    
    public class TvMediaCast
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MediaCastID { get; set; }
        public int MediaID { get; set; }
        public int TMDBPersonID { get; set; }
        public bool Adult { get; set; }
        public int Gender { get; set; }
        public int Id { get; set; }
        public string? KnownForDepartment { get; set; }
        public string? Name { get; set; }
        public string? OriginalName { get; set; }
        public double Popularity { get; set; }
        public string? ProfilePath { get; set; }
        public int? CastId { get; set; }
        public string? Character { get; set; }
        public string? CreditID { get; set; }
        public int? Order { get; set; }
    }

    public class TvMediaCrew
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MediaCrewID { get; set; }
        public int MediaID { get; set; }
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
    
    public class CrossRefVideoTvEpisode
    {
        public int CrossRefVideoTvEpisodeID { get; set; }
        public int VideoID { get; set; }
        public int TvEpisodeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual Video Video { get; set; }
        public virtual Episode TvEpisode { get; set; }
    }
    public class ExampleHash
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; }
        public string ED2K { get; set; }
    }


    public class TvExampleHash
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public int TvShowId { get; set; }
        
        public int SeasonNumber { get; set; }
        
        public int EpisodeNumber { get; set; }
        
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
        public virtual Profile? Profile { get; set; }
        public int? ProfileID { get; set; }

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

    public class TvRecommendationGenre
    {
        public int TvRecommendationID { get; set; }
        public int GenreID { get; set; }
        public string Name { get; set; }
        
        public virtual TvRecommendation TvRecommendation { get; set; }
        public virtual Genre Genre { get; set; }
    }
    public class WatchList
    {
        public int WatchListID { get; set; }
        public string Name { get; set; }

        public int? UserID { get; set; }
        public virtual User? User { get; set; }

        public int? ProfileID { get; set; }
        public virtual Profile? Profile { get; set; }

        public virtual ICollection<WatchListItem> Items { get; set; } = new List<WatchListItem>();
    }

    public class WatchListItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WatchListItemID { get; set; }
        public int WatchListID { get; set; }
        public virtual WatchList WatchList { get; set; }

        public int? MovieID { get; set; }
        public virtual TMDBMovie Movie { get; set; }

        public int? TvShowID { get; set; }

        public virtual TvShow TvShow { get; set; }

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
             
             if (DateTime.TryParse(api.FirstAirDate, out var parsedDate))
             {
                 FirstAirDate = parsedDate;
             }
             else
             {
                 FirstAirDate = null;
             }
             
             TvGenres = api.Genres?.Select(g => new TvGenre
             {
                 GenreID = g.id,
                 Name = g.name,
                 TvShowID = api.Id
             }).ToList() ?? new List<TvGenre>();
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
         
         public DateTime? FirstAirDate { get; set; }
         
         public double VoteAverage { get; set; }

         public int VoteCount { get; set; }
         
         public string? Certification { get; set; }
         
         public List<TvGenre> TvGenres { get; set; } = new List<TvGenre>();

         public List<Genre> Genres { get; set; } = new List<Genre>();

         public bool IsInCollection { get; set; } = false;

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
             EpisodeCount = api.EpisodeCount;
             Overview = api.Overview;
             PosterPath = api.PosterPath;
             VoteAverage = api.VoteAverage;

             if (DateTime.TryParse(api.Airdate, out var parsedDate))
             {
                 Airdate = parsedDate;
             }
             else
             {
                 Airdate = null;
             }
         }
         
         public Season(){}
         [Key]
         public int SeasonID { get; set; }
         
         [ForeignKey("TvShow")]
         public int TvShowID { get; set; }
         
         public string seasonName { get; set; }
         
         public virtual TvShow TvShow { get; set; }
         
         public int SeasonNumber { get; set; }
         
         public DateTime? Airdate { get; set; }
         
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
             Overview = api.Overview;
             SeasonNumber = api.SeasonNumber;
             Runtime = api.Runtime;
             VoteAverage = api.VoteAverage;
             VoteCount = api.VoteCount;
             StillPath = api.StillPath;

             if (DateTime.TryParse(api.Airdate, out var parsedDate))
             {
                 Airdate = parsedDate;
             }
             else
             {
                 Airdate = null;
             }
         }

         public Episode()
         {

         }

         [Key] public int EpisodeID { get; set; }

         [ForeignKey("Season")] public int SeasonID { get; set; }
         public virtual Season Season { get; set; }

         public string EpisodeName { get; set; }

         public int EpisodeNumber { get; set; }

         public DateTime? Airdate { get; set; }

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
         
         public string? BackdropPath { get; set; }
         
         public string? OriginalName { get; set; }
         
         public string? Overview { get; set; }
         
         public string? PosterPath { get; set; }
         
         
         public double? Popularity { get; set; }
         
         public string? firstAirDate { get; set; }
         
         public double? VoteAverage { get; set; }
         
         public string? Certification { get; set; }
         
         public int? VoteCount { get; set; }
         
         public virtual TvShow TvShow { get; set; }
         
         public List<TvRecommendationGenre> TvRecommendationGenres { get; set; } = new List<TvRecommendationGenre>();
         
         public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
     }

    public class Collection
    {
        public int CollectionID { get; set; }
        public string Name { get; set; }
        public string? PosterPath { get; set; }

        public int? UserID { get; set; }
        public virtual User? User { get; set; }
        public int? ProfileID { get; set; }
        public virtual Profile? Profile { get; set; }
        public virtual ICollection<CollectionItem> Items { get; set; } = new List<CollectionItem>();
    }

    public class CollectionItem
    {
        public int CollectionItemID { get; set; }

        public int CollectionID { get; set; }
        public virtual Collection Collection { get; set; }

        public int? TmdbMovieID { get; set; }
        public virtual TMDBMovie? TmdbMovie { get; set; }

        public int? TmdbTvID { get; set; }
        public virtual TvShow? TmdbTv { get; set; }
    }

    public class WatchStatistic
    {
        public int MovieID { get; set; }
        public int WatchDuration { get; set; }
    }

    public class Profile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProfileID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public bool Adult { get; set; }
        public string posterPath { get; set; }
        
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}