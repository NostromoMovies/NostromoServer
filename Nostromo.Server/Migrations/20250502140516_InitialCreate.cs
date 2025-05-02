using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DuplicateFiles",
                columns: table => new
                {
                    DuplicateFileID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FilePath1 = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath2 = table.Column<string>(type: "TEXT", nullable: false),
                    ImportFolderID = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportFolderType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateFiles", x => x.DuplicateFileID);
                });

            migrationBuilder.CreateTable(
                name: "ExampleHash",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TmdbId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    ED2K = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExampleHash", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    GenreID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => new { x.GenreID, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "ImportFolders",
                columns: table => new
                {
                    ImportFolderID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImportFolderType = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportFolderLocation = table.Column<string>(type: "TEXT", nullable: false),
                    IsDropSource = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDropDestination = table.Column<int>(type: "INTEGER", nullable: false),
                    IsWatched = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportFolders", x => x.ImportFolderID);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    TMDBMovieID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TMDBID = table.Column<int>(type: "INTEGER", nullable: false),
                    TMDBCollectionID = table.Column<int>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Overview = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalTitle = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalLanguage = table.Column<string>(type: "TEXT", nullable: false),
                    IsAdult = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVideo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Popularity = table.Column<float>(type: "REAL", nullable: false),
                    VoteAverage = table.Column<float>(type: "REAL", nullable: false),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Runtime = table.Column<int>(type: "INTEGER", nullable: false),
                    UserRating = table.Column<decimal>(type: "real", nullable: false),
                    UserVotes = table.Column<int>(type: "INTEGER", nullable: false),
                    ReleaseDate = table.Column<string>(type: "TEXT", nullable: false),
                    PosterPath = table.Column<string>(type: "TEXT", nullable: false),
                    BackdropPath = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Certification = table.Column<string>(type: "TEXT", nullable: true),
                    IsInCollection = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.TMDBMovieID);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    TMDBPersonID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TMDBID = table.Column<int>(type: "INTEGER", nullable: false),
                    EnglishName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Aliases = table.Column<string>(type: "TEXT", nullable: true),
                    EnglishBio = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<int>(type: "INTEGER", nullable: true),
                    IsRestricted = table.Column<bool>(type: "INTEGER", nullable: true, defaultValue: false),
                    BirthDay = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    DeathDay = table.Column<string>(type: "TEXT", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ProfilePath = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.TMDBPersonID);
                });

            migrationBuilder.CreateTable(
                name: "TvExampleHashes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TvShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    ED2K = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvExampleHashes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TvMediaCasts",
                columns: table => new
                {
                    MediaCastID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MediaID = table.Column<int>(type: "INTEGER", nullable: false),
                    TMDBPersonID = table.Column<int>(type: "INTEGER", nullable: false),
                    Adult = table.Column<bool>(type: "INTEGER", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    KnownForDepartment = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalName = table.Column<string>(type: "TEXT", nullable: true),
                    Popularity = table.Column<double>(type: "REAL", nullable: false),
                    ProfilePath = table.Column<string>(type: "TEXT", nullable: true),
                    CastId = table.Column<int>(type: "INTEGER", nullable: true),
                    Character = table.Column<string>(type: "TEXT", nullable: true),
                    CreditID = table.Column<string>(type: "TEXT", nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvMediaCasts", x => x.MediaCastID);
                });

            migrationBuilder.CreateTable(
                name: "TvMediaCrews",
                columns: table => new
                {
                    MediaCrewID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MediaID = table.Column<int>(type: "INTEGER", nullable: false),
                    TMDBPersonID = table.Column<int>(type: "INTEGER", nullable: false),
                    Adult = table.Column<bool>(type: "INTEGER", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    KnownForDepartment = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalName = table.Column<string>(type: "TEXT", nullable: true),
                    Popularity = table.Column<double>(type: "REAL", nullable: false),
                    ProfilePath = table.Column<string>(type: "TEXT", nullable: true),
                    CreditID = table.Column<string>(type: "TEXT", nullable: true),
                    Department = table.Column<string>(type: "TEXT", nullable: true),
                    Job = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvMediaCrews", x => x.MediaCrewID);
                });

            migrationBuilder.CreateTable(
                name: "TvShows",
                columns: table => new
                {
                    TvShowID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Adult = table.Column<bool>(type: "INTEGER", nullable: false),
                    OriginalLanguage = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalName = table.Column<string>(type: "TEXT", nullable: false),
                    Overview = table.Column<string>(type: "TEXT", nullable: false),
                    Popularity = table.Column<double>(type: "REAL", nullable: false),
                    PosterPath = table.Column<string>(type: "TEXT", nullable: true),
                    BackdropPath = table.Column<string>(type: "TEXT", nullable: true),
                    FirstAirDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VoteAverage = table.Column<double>(type: "REAL", nullable: false),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Certification = table.Column<string>(type: "TEXT", nullable: true),
                    IsInCollection = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvShows", x => x.TvShowID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Salt = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    VideoID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    ED2K = table.Column<string>(type: "TEXT", nullable: false),
                    CRC32 = table.Column<string>(type: "TEXT", nullable: false),
                    MD5 = table.Column<string>(type: "TEXT", nullable: false),
                    SHA1 = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsRecognized = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMovie = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTv = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.VideoID);
                });

            migrationBuilder.CreateTable(
                name: "WatchStatistics",
                columns: table => new
                {
                    MovieID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WatchDuration = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchStatistics", x => x.MovieID);
                });

            migrationBuilder.CreateTable(
                name: "MovieGenres",
                columns: table => new
                {
                    MovieID = table.Column<int>(type: "INTEGER", nullable: false),
                    GenreID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieGenres", x => new { x.MovieID, x.GenreID, x.Name });
                    table.ForeignKey(
                        name: "FK_MovieGenres_Genres_GenreID_Name",
                        columns: x => new { x.GenreID, x.Name },
                        principalTable: "Genres",
                        principalColumns: new[] { "GenreID", "Name" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieGenres_Movies_MovieID",
                        column: x => x.MovieID,
                        principalTable: "Movies",
                        principalColumn: "TMDBMovieID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieCasts",
                columns: table => new
                {
                    TMDBMovieCastID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TMDBMovieID = table.Column<int>(type: "INTEGER", nullable: false),
                    TMDBPersonID = table.Column<int>(type: "INTEGER", nullable: false),
                    Adult = table.Column<bool>(type: "INTEGER", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    KnownForDepartment = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalName = table.Column<string>(type: "TEXT", nullable: true),
                    Popularity = table.Column<double>(type: "REAL", nullable: false),
                    ProfilePath = table.Column<string>(type: "TEXT", nullable: true),
                    CastId = table.Column<int>(type: "INTEGER", nullable: false),
                    Character = table.Column<string>(type: "TEXT", nullable: true),
                    CreditID = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieCasts", x => x.TMDBMovieCastID);
                    table.ForeignKey(
                        name: "FK_MovieCasts_Movies_TMDBMovieID",
                        column: x => x.TMDBMovieID,
                        principalTable: "Movies",
                        principalColumn: "TMDBMovieID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieCasts_People_TMDBPersonID",
                        column: x => x.TMDBPersonID,
                        principalTable: "People",
                        principalColumn: "TMDBPersonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieCrews",
                columns: table => new
                {
                    TMDBMovieCrewID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TMDBMovieID = table.Column<int>(type: "INTEGER", nullable: false),
                    TMDBPersonID = table.Column<int>(type: "INTEGER", nullable: false),
                    Adult = table.Column<bool>(type: "INTEGER", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    KnownForDepartment = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalName = table.Column<string>(type: "TEXT", nullable: true),
                    Popularity = table.Column<double>(type: "REAL", nullable: false),
                    ProfilePath = table.Column<string>(type: "TEXT", nullable: true),
                    CreditID = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Department = table.Column<string>(type: "TEXT", nullable: true),
                    Job = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieCrews", x => x.TMDBMovieCrewID);
                    table.ForeignKey(
                        name: "FK_MovieCrews_Movies_TMDBMovieID",
                        column: x => x.TMDBMovieID,
                        principalTable: "Movies",
                        principalColumn: "TMDBMovieID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieCrews_People_TMDBPersonID",
                        column: x => x.TMDBPersonID,
                        principalTable: "People",
                        principalColumn: "TMDBPersonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    SeasonID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TvShowID = table.Column<int>(type: "INTEGER", nullable: false),
                    seasonName = table.Column<string>(type: "TEXT", nullable: false),
                    SeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Airdate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EpisodeCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Overview = table.Column<string>(type: "TEXT", nullable: false),
                    PosterPath = table.Column<string>(type: "TEXT", nullable: false),
                    VoteAverage = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.SeasonID);
                    table.ForeignKey(
                        name: "FK_Seasons_TvShows_TvShowID",
                        column: x => x.TvShowID,
                        principalTable: "TvShows",
                        principalColumn: "TvShowID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TvGenres",
                columns: table => new
                {
                    TvShowID = table.Column<int>(type: "INTEGER", nullable: false),
                    GenreID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvGenres", x => new { x.TvShowID, x.GenreID, x.Name });
                    table.ForeignKey(
                        name: "FK_TvGenres_Genres_GenreID_Name",
                        columns: x => new { x.GenreID, x.Name },
                        principalTable: "Genres",
                        principalColumns: new[] { "GenreID", "Name" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TvGenres_TvShows_TvShowID",
                        column: x => x.TvShowID,
                        principalTable: "TvShows",
                        principalColumn: "TvShowID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TvRecommendations",
                columns: table => new
                {
                    RecommendationID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    ShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Adult = table.Column<bool>(type: "INTEGER", nullable: false),
                    BackdropPath = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalName = table.Column<string>(type: "TEXT", nullable: true),
                    Overview = table.Column<string>(type: "TEXT", nullable: true),
                    PosterPath = table.Column<string>(type: "TEXT", nullable: true),
                    Popularity = table.Column<double>(type: "REAL", nullable: true),
                    firstAirDate = table.Column<string>(type: "TEXT", nullable: true),
                    VoteAverage = table.Column<double>(type: "REAL", nullable: true),
                    Certification = table.Column<string>(type: "TEXT", nullable: true),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvRecommendations", x => x.RecommendationID);
                    table.ForeignKey(
                        name: "FK_TvRecommendations_TvShows_ShowId",
                        column: x => x.ShowId,
                        principalTable: "TvShows",
                        principalColumn: "TvShowID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthTokens",
                columns: table => new
                {
                    AuthId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceName = table.Column<string>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthTokens", x => x.AuthId);
                    table.ForeignKey(
                        name: "FK_AuthTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    ProfileID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    Adult = table.Column<bool>(type: "INTEGER", nullable: false),
                    posterPath = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.ProfileID);
                    table.ForeignKey(
                        name: "FK_Profiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrossRefVideoTMDBMovies",
                columns: table => new
                {
                    CrossRefVideoTMDBMovieID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoID = table.Column<int>(type: "INTEGER", nullable: false),
                    TMDBMovieID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrossRefVideoTMDBMovies", x => x.CrossRefVideoTMDBMovieID);
                    table.ForeignKey(
                        name: "FK_CrossRefVideoTMDBMovies_Movies_TMDBMovieID",
                        column: x => x.TMDBMovieID,
                        principalTable: "Movies",
                        principalColumn: "TMDBMovieID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrossRefVideoTMDBMovies_Videos_VideoID",
                        column: x => x.VideoID,
                        principalTable: "Videos",
                        principalColumn: "VideoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoPlaces",
                columns: table => new
                {
                    VideoPlaceID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoID = table.Column<int>(type: "INTEGER", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    ImportFolderID = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportFolderType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoPlaces", x => x.VideoPlaceID);
                    table.ForeignKey(
                        name: "FK_VideoPlaces_Videos_VideoID",
                        column: x => x.VideoID,
                        principalTable: "Videos",
                        principalColumn: "VideoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    EpisodeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeasonID = table.Column<int>(type: "INTEGER", nullable: false),
                    EpisodeName = table.Column<string>(type: "TEXT", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Airdate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Overview = table.Column<string>(type: "TEXT", nullable: false),
                    SeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Runtime = table.Column<int>(type: "INTEGER", nullable: false),
                    VoteAverage = table.Column<double>(type: "REAL", nullable: false),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: false),
                    StillPath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.EpisodeID);
                    table.ForeignKey(
                        name: "FK_Episodes_Seasons_SeasonID",
                        column: x => x.SeasonID,
                        principalTable: "Seasons",
                        principalColumn: "SeasonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenreTvRecommendation",
                columns: table => new
                {
                    TvRecommendationsRecommendationID = table.Column<int>(type: "INTEGER", nullable: false),
                    GenresGenreID = table.Column<int>(type: "INTEGER", nullable: false),
                    GenresName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenreTvRecommendation", x => new { x.TvRecommendationsRecommendationID, x.GenresGenreID, x.GenresName });
                    table.ForeignKey(
                        name: "FK_GenreTvRecommendation_Genres_GenresGenreID_GenresName",
                        columns: x => new { x.GenresGenreID, x.GenresName },
                        principalTable: "Genres",
                        principalColumns: new[] { "GenreID", "Name" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenreTvRecommendation_TvRecommendations_TvRecommendationsRecommendationID",
                        column: x => x.TvRecommendationsRecommendationID,
                        principalTable: "TvRecommendations",
                        principalColumn: "RecommendationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TvRecommendationGenres",
                columns: table => new
                {
                    TvRecommendationID = table.Column<int>(type: "INTEGER", nullable: false),
                    GenreID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvRecommendationGenres", x => new { x.TvRecommendationID, x.GenreID, x.Name });
                    table.ForeignKey(
                        name: "FK_TvRecommendationGenres_Genres_GenreID_Name",
                        columns: x => new { x.GenreID, x.Name },
                        principalTable: "Genres",
                        principalColumns: new[] { "GenreID", "Name" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TvRecommendationGenres_TvRecommendations_TvRecommendationID",
                        column: x => x.TvRecommendationID,
                        principalTable: "TvRecommendations",
                        principalColumn: "RecommendationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    CollectionID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PosterPath = table.Column<string>(type: "TEXT", nullable: true),
                    UserID = table.Column<int>(type: "INTEGER", nullable: true),
                    ProfileID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.CollectionID);
                    table.ForeignKey(
                        name: "FK_Collections_Profiles_ProfileID",
                        column: x => x.ProfileID,
                        principalTable: "Profiles",
                        principalColumn: "ProfileID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Collections_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    RecommendationID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    TMDBMovieID = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Overview = table.Column<string>(type: "TEXT", nullable: false),
                    PosterPath = table.Column<string>(type: "TEXT", nullable: false),
                    BackdropPath = table.Column<string>(type: "TEXT", nullable: false),
                    MediaType = table.Column<string>(type: "TEXT", nullable: false),
                    Adult = table.Column<bool>(type: "INTEGER", nullable: false),
                    OriginalLanguage = table.Column<string>(type: "TEXT", nullable: false),
                    Popularity = table.Column<double>(type: "REAL", nullable: false),
                    ReleaseDate = table.Column<string>(type: "TEXT", nullable: false),
                    Video = table.Column<bool>(type: "INTEGER", nullable: false),
                    VoteAverage = table.Column<double>(type: "REAL", nullable: false),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ProfileID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.RecommendationID);
                    table.ForeignKey(
                        name: "FK_Recommendations_Movies_TMDBMovieID",
                        column: x => x.TMDBMovieID,
                        principalTable: "Movies",
                        principalColumn: "TMDBMovieID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Recommendations_Profiles_ProfileID",
                        column: x => x.ProfileID,
                        principalTable: "Profiles",
                        principalColumn: "ProfileID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WatchLists",
                columns: table => new
                {
                    WatchListID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    UserID = table.Column<int>(type: "INTEGER", nullable: true),
                    ProfileID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchLists", x => x.WatchListID);
                    table.ForeignKey(
                        name: "FK_WatchLists_Profiles_ProfileID",
                        column: x => x.ProfileID,
                        principalTable: "Profiles",
                        principalColumn: "ProfileID");
                    table.ForeignKey(
                        name: "FK_WatchLists_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrossRefVideoTvEpisodes",
                columns: table => new
                {
                    CrossRefVideoTvEpisodeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoID = table.Column<int>(type: "INTEGER", nullable: false),
                    TvEpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrossRefVideoTvEpisodes", x => x.CrossRefVideoTvEpisodeID);
                    table.ForeignKey(
                        name: "FK_CrossRefVideoTvEpisodes_Episodes_TvEpisodeId",
                        column: x => x.TvEpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "EpisodeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrossRefVideoTvEpisodes_Videos_VideoID",
                        column: x => x.VideoID,
                        principalTable: "Videos",
                        principalColumn: "VideoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionItems",
                columns: table => new
                {
                    CollectionItemID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CollectionID = table.Column<int>(type: "INTEGER", nullable: false),
                    TmdbMovieID = table.Column<int>(type: "INTEGER", nullable: true),
                    TmdbTvID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionItems", x => x.CollectionItemID);
                    table.ForeignKey(
                        name: "FK_CollectionItems_Collections_CollectionID",
                        column: x => x.CollectionID,
                        principalTable: "Collections",
                        principalColumn: "CollectionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionItems_Movies_TmdbMovieID",
                        column: x => x.TmdbMovieID,
                        principalTable: "Movies",
                        principalColumn: "TMDBMovieID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionItems_TvShows_TmdbTvID",
                        column: x => x.TmdbTvID,
                        principalTable: "TvShows",
                        principalColumn: "TvShowID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationGenres",
                columns: table => new
                {
                    RecommendationID = table.Column<int>(type: "INTEGER", nullable: false),
                    GenreID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationGenres", x => new { x.RecommendationID, x.GenreID, x.Name });
                    table.ForeignKey(
                        name: "FK_RecommendationGenres_Genres_GenreID_Name",
                        columns: x => new { x.GenreID, x.Name },
                        principalTable: "Genres",
                        principalColumns: new[] { "GenreID", "Name" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecommendationGenres_Recommendations_RecommendationID",
                        column: x => x.RecommendationID,
                        principalTable: "Recommendations",
                        principalColumn: "RecommendationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatchListItems",
                columns: table => new
                {
                    WatchListItemID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WatchListID = table.Column<int>(type: "INTEGER", nullable: false),
                    MovieID = table.Column<int>(type: "INTEGER", nullable: true),
                    TvShowID = table.Column<int>(type: "INTEGER", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchListItems", x => x.WatchListItemID);
                    table.CheckConstraint("CK_WatchListItem_MovieOrTvShow", "(MovieID IS NOT NULL AND TvShowID IS NULL) OR (MovieID IS NULL AND TvShowID IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_WatchListItems_Movies_MovieID",
                        column: x => x.MovieID,
                        principalTable: "Movies",
                        principalColumn: "TMDBMovieID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WatchListItems_TvShows_TvShowID",
                        column: x => x.TvShowID,
                        principalTable: "TvShows",
                        principalColumn: "TvShowID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WatchListItems_WatchLists_WatchListID",
                        column: x => x.WatchListID,
                        principalTable: "WatchLists",
                        principalColumn: "WatchListID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ExampleHash",
                columns: new[] { "Id", "ED2K", "Title", "TmdbId" },
                values: new object[,]
                {
                    { 1, "5d886780825db91bbc390f10f1b6c95c", "Alien", 348 },
                    { 2, "da1a506c0ee1fe6c46ec64fd57faa924", "Aliens", 679 },
                    { 3, "b33d9c30eb480eca99e82dbbab3aad0e", "Alien 3", 8077 },
                    { 4, "b8b18d2129c23ce7be0f20192ab5cc7d", "2001: A Space Odyssey", 62 },
                    { 5, "f1f92c24015ee61c26ee14e1a620c2f1", "Blade Runner", 78 },
                    { 6, "5c397afacb0a7e4ca53a208c70a60312", "Close Encounters of the Third Kind", 840 },
                    { 7, "bd48b94f65b1cb6526acc0cd0b52e733", "Big Hero 6", 177572 },
                    { 8, "2bc38b9668690f1b5d83bcd5d0b8875c", "Arrival", 329865 },
                    { 9, "aa9f497e20846c5018f47e025d06d190", "A.I. Artificial Intelligence", 644 },
                    { 10, "6dc784b7b42faa32d70106b6008137fc", "Blade Runner 2049", 335984 },
                    { 11, "8891dba5d7423e41be1670f5022514a6", "Flight of the Navigator", 10122 },
                    { 12, "77ec11a8b08ee689cb4e8e9cbae406fb", "The Iron Giant", 10386 },
                    { 13, "cde961b6799ad092ffe00e17ebd95cdb", "Meet The Robinsons", 1267 },
                    { 14, "e16a3334eaa4a1b36c7ffb0eb2ec0c35", "Event Horizon", 8413 },
                    { 15, "89d725b0be5df4643edcaca155ecf165", "Lilo & Stitch", 11544 },
                    { 16, "4ca3e7ad70bd6595ee68fabfd0273534", "E.T. The Extra Terrestrial", 601 },
                    { 17, "a60bc42199d8a34638087b267bea1400", "The Thing", 1091 },
                    { 18, "f69fa1b76e69c8141e52945175bd81d0", "The Last Starfighter", 11884 },
                    { 19, "b092919efab8f3c27e5e67cf15a02acd", "Treasure Planet", 9016 },
                    { 20, "8ca300a5aa1a73c8419f4d1622c3364d", "WALL-E", 10681 },
                    { 21, "c0c717a4f8fad3366520d47c702ab5ad", "Total Recall", 861 },
                    { 22, "d68b4df74882553d70ddf8b1bfa4c510", "Altered States", 11542 },
                    { 23, "da7b55d4b775b92635a34b10ef30ec88", "Con Air", 1701 },
                    { 24, "e0dbd16993d8305df99162d76d196942", "Dracula's Daughter", 22440 },
                    { 25, "d66e8ed433fbadb4dd7a2563e07e5133", "Drunken Master", 11230 },
                    { 26, "07321dbd990d80af952132132f3aaba7", "I Stand Alone", 1567 },
                    { 27, "a41745e62f6b2f0c9908ce278e817e77", "Mad God", 846867 },
                    { 28, "bb8dcf1c11c68c414631bf1b2e861a25", "Journey To The Center Of The Earth", 88751 },
                    { 29, "8dac63001e29e8e9410f1d22ccd5eedc", "Paprika", 4977 },
                    { 30, "50cb41f65ff732f40d1f52e7758cff15", "Q - The Winged Serpent", 27726 },
                    { 31, "b11eabf34f194c2429247bd699918d73", "Little Big Man", 11040 },
                    { 32, "14e1ec9339fcc1a2b0754a288acee7ce", "Harakiri", 14537 },
                    { 33, "bf80cbaadf242c4043d44385de75638d", "Rons Gone Wrong", 482321 },
                    { 34, "a915eb32bff0604eec9afc47806b7a09", "Strangers on a Train", 845 },
                    { 35, "bbd17e2e0e39f177fb42ba12fbc3a397", "Rio Bravo", 301 },
                    { 36, "ff4864650ec980159573479b67151859", "The Omega Man", 1450 },
                    { 37, "53b3060dde08445467d73e8e07976f83", "The Pink Panther", 936 },
                    { 38, "47c1596a0d6c657cc0f030593156eb9d", "Tremors", 9362 },
                    { 39, "7b76844584faec9599909bf10113d9cf", "The Skin I Live In", 63311 },
                    { 40, "10d831b6f8db5f6eaaa35d485d9b38e5", "The Talented Mr. Ripley", 1213 },
                    { 41, "548e716ce84608f6aabe0aaeb23ad855", "Young Frankenstein", 3034 }
                });

            migrationBuilder.InsertData(
                table: "TvExampleHashes",
                columns: new[] { "Id", "ED2K", "EpisodeNumber", "SeasonNumber", "Title", "TvShowId" },
                values: new object[,]
                {
                    { 1, "a413da8e3e3bb02237795b2dc9e06b8d", 1, 1, "The Blacklist", 46952 },
                    { 2, "ee4a746481ec4a6a909943562aefe86a", 2, 1, "The Blacklist", 46952 },
                    { 3, "a73c8cf075a960af6004a257432b2435", 3, 1, "The Blacklist", 46952 },
                    { 4, "ea85563f8f9c051cab70a0139c5118da", 4, 1, "The Blacklist", 46952 },
                    { 5, "c5f51c3dc5b4b45c68e428ccc062949f", 5, 1, "The Blacklist", 46952 },
                    { 6, "7203ced34b4989a4527457a4c564e2c1", 1, 2, "The Blacklist", 46952 },
                    { 7, "8accb9f07416005acdd4d4d9bc790295", 2, 2, "The Blacklist", 46952 },
                    { 8, "41da21faa145d66664535b5084240096", 3, 2, "The Blacklist", 46952 },
                    { 9, "2bda47a34c226363615c0355e001683b", 4, 2, "The Blacklist", 46952 },
                    { 10, "15f73bad52cd5ce13a95673e90708939", 1, 1, "The National Anthem", 42009 },
                    { 11, "1f0103e25e21ae6b3092a3a53c91f21b", 2, 1, "Fifteen Million Merits", 42009 },
                    { 12, "4dc74beecc6eb8937b540ff4a51a8bea", 3, 1, "The Entire History of You", 42009 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthTokens_UserId",
                table: "AuthTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_CollectionID",
                table: "CollectionItems",
                column: "CollectionID");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_TmdbMovieID",
                table: "CollectionItems",
                column: "TmdbMovieID");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_TmdbTvID",
                table: "CollectionItems",
                column: "TmdbTvID");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ProfileID",
                table: "Collections",
                column: "ProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_UserID",
                table: "Collections",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTMDBMovies_TMDBMovieID",
                table: "CrossRefVideoTMDBMovies",
                column: "TMDBMovieID");

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTMDBMovies_VideoID",
                table: "CrossRefVideoTMDBMovies",
                column: "VideoID");

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTvEpisodes_TvEpisodeId",
                table: "CrossRefVideoTvEpisodes",
                column: "TvEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTvEpisodes_VideoID",
                table: "CrossRefVideoTvEpisodes",
                column: "VideoID");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_SeasonID",
                table: "Episodes",
                column: "SeasonID");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_GenreID_Name",
                table: "Genres",
                columns: new[] { "GenreID", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GenreTvRecommendation_GenresGenreID_GenresName",
                table: "GenreTvRecommendation",
                columns: new[] { "GenresGenreID", "GenresName" });

            migrationBuilder.CreateIndex(
                name: "IX_MovieCasts_TMDBMovieID",
                table: "MovieCasts",
                column: "TMDBMovieID");

            migrationBuilder.CreateIndex(
                name: "IX_MovieCasts_TMDBPersonID",
                table: "MovieCasts",
                column: "TMDBPersonID");

            migrationBuilder.CreateIndex(
                name: "IX_MovieCrews_TMDBMovieID",
                table: "MovieCrews",
                column: "TMDBMovieID");

            migrationBuilder.CreateIndex(
                name: "IX_MovieCrews_TMDBPersonID",
                table: "MovieCrews",
                column: "TMDBPersonID");

            migrationBuilder.CreateIndex(
                name: "IX_MovieGenres_GenreID_Name",
                table: "MovieGenres",
                columns: new[] { "GenreID", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_People_TMDBID",
                table: "People",
                column: "TMDBID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_UserId",
                table: "Profiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationGenres_GenreID_Name",
                table: "RecommendationGenres",
                columns: new[] { "GenreID", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_ProfileID",
                table: "Recommendations",
                column: "ProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_TMDBMovieID_Id",
                table: "Recommendations",
                columns: new[] { "TMDBMovieID", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_TvShowID",
                table: "Seasons",
                column: "TvShowID");

            migrationBuilder.CreateIndex(
                name: "IX_TvGenres_GenreID_Name",
                table: "TvGenres",
                columns: new[] { "GenreID", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_TvRecommendationGenres_GenreID_Name",
                table: "TvRecommendationGenres",
                columns: new[] { "GenreID", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_TvRecommendations_ShowId",
                table: "TvRecommendations",
                column: "ShowId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoPlaces_VideoID",
                table: "VideoPlaces",
                column: "VideoID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchListItems_MovieID",
                table: "WatchListItems",
                column: "MovieID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchListItems_TvShowID",
                table: "WatchListItems",
                column: "TvShowID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchListItems_WatchListID",
                table: "WatchListItems",
                column: "WatchListID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchLists_ProfileID",
                table: "WatchLists",
                column: "ProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchLists_UserID",
                table: "WatchLists",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthTokens");

            migrationBuilder.DropTable(
                name: "CollectionItems");

            migrationBuilder.DropTable(
                name: "CrossRefVideoTMDBMovies");

            migrationBuilder.DropTable(
                name: "CrossRefVideoTvEpisodes");

            migrationBuilder.DropTable(
                name: "DuplicateFiles");

            migrationBuilder.DropTable(
                name: "ExampleHash");

            migrationBuilder.DropTable(
                name: "GenreTvRecommendation");

            migrationBuilder.DropTable(
                name: "ImportFolders");

            migrationBuilder.DropTable(
                name: "MovieCasts");

            migrationBuilder.DropTable(
                name: "MovieCrews");

            migrationBuilder.DropTable(
                name: "MovieGenres");

            migrationBuilder.DropTable(
                name: "RecommendationGenres");

            migrationBuilder.DropTable(
                name: "TvExampleHashes");

            migrationBuilder.DropTable(
                name: "TvGenres");

            migrationBuilder.DropTable(
                name: "TvMediaCasts");

            migrationBuilder.DropTable(
                name: "TvMediaCrews");

            migrationBuilder.DropTable(
                name: "TvRecommendationGenres");

            migrationBuilder.DropTable(
                name: "VideoPlaces");

            migrationBuilder.DropTable(
                name: "WatchListItems");

            migrationBuilder.DropTable(
                name: "WatchStatistics");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "TvRecommendations");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "WatchLists");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "TvShows");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
