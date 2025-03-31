﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nostromo.Server.Database;

#nullable disable

namespace Nostromo.Server.Migrations
{
    [DbContext(typeof(NostromoDbContext))]
    [Migration("20250314050352_UpdateDatabase")]
    partial class UpdateDatabase
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("Nostromo.Server.Database.AuthToken", b =>
                {
                    b.Property<int>("AuthId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DeviceName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("AuthId");

                    b.HasIndex("UserId");

                    b.ToTable("AuthTokens");
                });

            modelBuilder.Entity("Nostromo.Server.Database.CrossRefVideoTMDBMovie", b =>
                {
                    b.Property<int>("CrossRefVideoTMDBMovieID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("TMDBMovieID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VideoID")
                        .HasColumnType("INTEGER");

                    b.HasKey("CrossRefVideoTMDBMovieID");

                    b.HasIndex("TMDBMovieID");

                    b.HasIndex("VideoID");

                    b.ToTable("CrossRefVideoTMDBMovies");
                });

            modelBuilder.Entity("Nostromo.Server.Database.DuplicateFile", b =>
                {
                    b.Property<int>("DuplicateFileID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FilePath1")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FilePath2")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ImportFolderID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ImportFolderType")
                        .HasColumnType("INTEGER");

                    b.HasKey("DuplicateFileID");

                    b.ToTable("DuplicateFiles");
                });

            modelBuilder.Entity("Nostromo.Server.Database.Episode", b =>
                {
                    b.Property<int>("EpisodeID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("MediaTypeMediaTmDBID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SeasonID")
                        .HasColumnType("INTEGER");

                    b.HasKey("EpisodeID");

                    b.HasIndex("MediaTypeMediaTmDBID");

                    b.HasIndex("SeasonID");

                    b.ToTable("Episodes");
                });

            modelBuilder.Entity("Nostromo.Server.Database.ExampleHash", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ED2K")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("TmdbId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ExampleHash");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            ED2K = "5d886780825db91bbc390f10f1b6c95c",
                            Title = "Alien",
                            TmdbId = 348
                        },
                        new
                        {
                            Id = 2,
                            ED2K = "da1a506c0ee1fe6c46ec64fd57faa924",
                            Title = "Aliens",
                            TmdbId = 679
                        },
                        new
                        {
                            Id = 3,
                            ED2K = "b33d9c30eb480eca99e82dbbab3aad0e",
                            Title = "Alien 3",
                            TmdbId = 8077
                        });
                });

            modelBuilder.Entity("Nostromo.Server.Database.Genre", b =>
                {
                    b.Property<int>("GenreID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("GenreID");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("Nostromo.Server.Database.ImportFolder", b =>
                {
                    b.Property<int>("ImportFolderID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FolderLocation")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("ImportFolderLocation");

                    b.Property<int>("ImportFolderType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("IsDropDestination")
                        .HasColumnType("INTEGER");

                    b.Property<int>("IsDropSource")
                        .HasColumnType("INTEGER");

                    b.Property<int>("IsWatched")
                        .HasColumnType("INTEGER");

                    b.HasKey("ImportFolderID");

                    b.ToTable("ImportFolders");
                });

            modelBuilder.Entity("Nostromo.Server.Database.MediaType", b =>
                {
                    b.Property<int>("MediaTmDBID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MediaTypeName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("MediaTmDBID");

                    b.ToTable("MediaType");
                });

            modelBuilder.Entity("Nostromo.Server.Database.MovieGenre", b =>
                {
                    b.Property<int>("GenreID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MovieID")
                        .HasColumnType("INTEGER");

                    b.HasKey("GenreID", "MovieID");

                    b.HasIndex("MovieID");

                    b.ToTable("MovieGenre");
                });

            modelBuilder.Entity("Nostromo.Server.Database.Season", b =>
                {
                    b.Property<int>("SeasonID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("TvShowID")
                        .HasColumnType("INTEGER");

                    b.HasKey("SeasonID");

                    b.HasIndex("TvShowID");

                    b.ToTable("Seasons");
                });

            modelBuilder.Entity("Nostromo.Server.Database.TMDBMovie", b =>
                {
                    b.Property<int>("MovieID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("TMDBMovieID");

                    b.Property<string>("BackdropPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("CreatedAt")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsAdult")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsVideo")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastUpdatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("MediaTypeMediaTmDBID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OriginalLanguage")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalTitle")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Overview")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<float>("Popularity")
                        .HasColumnType("REAL");

                    b.Property<string>("PosterPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ReleaseDate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Runtime")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TMDBCollectionID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TMDBID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("UserRating")
                        .HasColumnType("real");

                    b.Property<int>("UserVotes")
                        .HasColumnType("INTEGER");

                    b.Property<float>("VoteAverage")
                        .HasColumnType("REAL");

                    b.Property<int>("VoteCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("MovieID");

                    b.HasIndex("MediaTypeMediaTmDBID");

                    b.ToTable("Movies");
                });

            modelBuilder.Entity("Nostromo.Server.Database.TMDBMovieCast", b =>
                {
                    b.Property<int>("TMDBMovieCastID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Adult")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CastId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Character")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreditID")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("Gender")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("KnownForDepartment")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Order")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OriginalName")
                        .HasColumnType("TEXT");

                    b.Property<double>("Popularity")
                        .HasColumnType("REAL");

                    b.Property<string>("ProfilePath")
                        .HasColumnType("TEXT");

                    b.Property<int>("TMDBMovieID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TMDBPersonID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TMDBMovieCastID");

                    b.HasIndex("TMDBMovieID");

                    b.HasIndex("TMDBPersonID");

                    b.ToTable("MovieCasts");
                });

            modelBuilder.Entity("Nostromo.Server.Database.TMDBMovieCrew", b =>
                {
                    b.Property<int>("TMDBMovieCrewID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Adult")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreditID")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Department")
                        .HasColumnType("TEXT");

                    b.Property<int>("Gender")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Job")
                        .HasColumnType("TEXT");

                    b.Property<string>("KnownForDepartment")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalName")
                        .HasColumnType("TEXT");

                    b.Property<double>("Popularity")
                        .HasColumnType("REAL");

                    b.Property<string>("ProfilePath")
                        .HasColumnType("TEXT");

                    b.Property<int>("TMDBMovieID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TMDBPersonID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TMDBMovieCrewID");

                    b.HasIndex("TMDBMovieID");

                    b.HasIndex("TMDBPersonID");

                    b.ToTable("MovieCrews");
                });

            modelBuilder.Entity("Nostromo.Server.Database.TMDBPerson", b =>
                {
                    b.Property<int>("TMDBPersonID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Aliases")
                        .HasColumnType("TEXT")
                        .HasColumnName("Aliases");

                    b.Property<string>("BirthDay")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("DeathDay")
                        .HasColumnType("TEXT");

                    b.Property<string>("EnglishBio")
                        .HasColumnType("TEXT");

                    b.Property<string>("EnglishName")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<int?>("Gender")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("IsRestricted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(false);

                    b.Property<DateTime>("LastUpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("PlaceOfBirth")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("ProfilePath")
                        .HasColumnType("TEXT");

                    b.Property<int>("TMDBID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TMDBPersonID");

                    b.ToTable("People");
                });

            modelBuilder.Entity("Nostromo.Server.Database.TMDBRecommendation", b =>
                {
                    b.Property<int>("RecommendationID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Adult")
                        .HasColumnType("INTEGER");

                    b.Property<string>("BackdropPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("GenreIds")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MediaType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalLanguage")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalTitle")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Overview")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Popularity")
                        .HasColumnType("REAL");

                    b.Property<string>("PosterPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ReleaseDate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("TMDBMovieID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Video")
                        .HasColumnType("INTEGER");

                    b.Property<double>("VoteAverage")
                        .HasColumnType("REAL");

                    b.Property<int>("VoteCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("RecommendationID");

                    b.HasIndex("TMDBMovieID", "Id");

                    b.ToTable("Recommendations");
                });

            modelBuilder.Entity("Nostromo.Server.Database.TvShow", b =>
                {
                    b.Property<int>("TvShowID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Adult")
                        .HasColumnType("INTEGER");

                    b.Property<string>("BackdropPath")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("FirstAirDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalLanguage")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Overview")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Popularity")
                        .HasColumnType("REAL");

                    b.Property<string>("PosterPath")
                        .HasColumnType("TEXT");

                    b.Property<double>("VoteAverage")
                        .HasColumnType("REAL");

                    b.Property<int>("VoteCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("TvShowID");

                    b.ToTable("TvShows");
                });

            modelBuilder.Entity("Nostromo.Server.Database.User", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Nostromo.Server.Database.Video", b =>
                {
                    b.Property<int>("VideoID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CRC32")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("ED2K")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("FileSize")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsRecognized")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MD5")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SHA1")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("VideoID");

                    b.ToTable("Videos");
                });

            modelBuilder.Entity("Nostromo.Server.Database.VideoPlace", b =>
                {
                    b.Property<int>("VideoPlaceID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ImportFolderID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ImportFolderType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VideoID")
                        .HasColumnType("INTEGER");

                    b.HasKey("VideoPlaceID");

                    b.HasIndex("VideoID");

                    b.ToTable("VideoPlaces");
                });

            modelBuilder.Entity("Nostromo.Server.Database.AuthToken", b =>
                {
                    b.HasOne("Nostromo.Server.Database.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Nostromo.Server.Database.CrossRefVideoTMDBMovie", b =>
                {
                    b.HasOne("Nostromo.Server.Database.TMDBMovie", "TMDBMovie")
                        .WithMany()
                        .HasForeignKey("TMDBMovieID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Nostromo.Server.Database.Video", "Video")
                        .WithMany()
                        .HasForeignKey("VideoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TMDBMovie");

                    b.Navigation("Video");
                });

            modelBuilder.Entity("Nostromo.Server.Database.Episode", b =>
                {
                    b.HasOne("Nostromo.Server.Database.MediaType", "MediaType")
                        .WithMany()
                        .HasForeignKey("MediaTypeMediaTmDBID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Nostromo.Server.Database.Season", "Season")
                        .WithMany()
                        .HasForeignKey("SeasonID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MediaType");

                    b.Navigation("Season");
                });

            modelBuilder.Entity("Nostromo.Server.Database.MovieGenre", b =>
                {
                    b.HasOne("Nostromo.Server.Database.Genre", "Genre")
                        .WithMany()
                        .HasForeignKey("GenreID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Nostromo.Server.Database.TMDBMovie", "Movie")
                        .WithMany()
                        .HasForeignKey("MovieID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Genre");

                    b.Navigation("Movie");
                });

            modelBuilder.Entity("Nostromo.Server.Database.Season", b =>
                {
                    b.HasOne("Nostromo.Server.Database.TvShow", "TvShow")
                        .WithMany()
                        .HasForeignKey("TvShowID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TvShow");
                });

            modelBuilder.Entity("Nostromo.Server.Database.TMDBMovie", b =>
                {
                    b.HasOne("Nostromo.Server.Database.MediaType", "MediaType")
                        .WithMany()
                        .HasForeignKey("MediaTypeMediaTmDBID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MediaType");
                });

            modelBuilder.Entity("Nostromo.Server.Database.TMDBMovieCast", b =>
                {
                    b.HasOne("Nostromo.Server.Database.TMDBMovie", null)
                        .WithMany()
                        .HasForeignKey("TMDBMovieID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Nostromo.Server.Database.TMDBPerson", null)
                        .WithMany()
                        .HasForeignKey("TMDBPersonID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Nostromo.Server.Database.TMDBMovieCrew", b =>
                {
                    b.HasOne("Nostromo.Server.Database.TMDBMovie", null)
                        .WithMany()
                        .HasForeignKey("TMDBMovieID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Nostromo.Server.Database.TMDBPerson", null)
                        .WithMany()
                        .HasForeignKey("TMDBPersonID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Nostromo.Server.Database.TMDBRecommendation", b =>
                {
                    b.HasOne("Nostromo.Server.Database.TMDBMovie", "Movie")
                        .WithMany()
                        .HasForeignKey("TMDBMovieID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Movie");
                });

            modelBuilder.Entity("Nostromo.Server.Database.VideoPlace", b =>
                {
                    b.HasOne("Nostromo.Server.Database.Video", "Video")
                        .WithMany()
                        .HasForeignKey("VideoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Video");
                });
#pragma warning restore 612, 618
        }
    }
}
