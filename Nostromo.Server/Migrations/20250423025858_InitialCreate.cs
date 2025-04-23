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
                name: "Collections",
                columns: table => new
                {
                    CollectionID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.CollectionID);
                });

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
                    FirstAirDate = table.Column<string>(type: "TEXT", nullable: true),
                    VoteAverage = table.Column<double>(type: "REAL", nullable: false),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: false),
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
                    IsRecognized = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.VideoID);
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
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.RecommendationID);
                    table.ForeignKey(
                        name: "FK_Recommendations_Movies_TMDBMovieID",
                        column: x => x.TMDBMovieID,
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
                name: "Seasons",
                columns: table => new
                {
                    SeasonID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TvShowID = table.Column<int>(type: "INTEGER", nullable: false),
                    seasonName = table.Column<string>(type: "TEXT", nullable: false),
                    SeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Airdate = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "TvRecommendations",
                columns: table => new
                {
                    RecommendationID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    ShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Adult = table.Column<bool>(type: "INTEGER", nullable: false),
                    BackdropPath = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalName = table.Column<string>(type: "TEXT", nullable: false),
                    Overview = table.Column<string>(type: "TEXT", nullable: false),
                    PosterPath = table.Column<string>(type: "TEXT", nullable: false),
                    MediaType = table.Column<string>(type: "TEXT", nullable: false),
                    Popularity = table.Column<double>(type: "REAL", nullable: false),
                    firstAirDate = table.Column<string>(type: "TEXT", nullable: false),
                    VoteAverage = table.Column<double>(type: "REAL", nullable: false),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TvShowID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvRecommendations", x => x.RecommendationID);
                    table.ForeignKey(
                        name: "FK_TvRecommendations_TvShows_TvShowID",
                        column: x => x.TvShowID,
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
                name: "WatchLists",
                columns: table => new
                {
                    WatchListID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    UserID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchLists", x => x.WatchListID);
                    table.ForeignKey(
                        name: "FK_WatchLists_Users_UserID",
                        column: x => x.UserID,
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
                    Airdate = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "Genres",
                columns: table => new
                {
                    GenreID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    TvRecommendationRecommendationID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => new { x.GenreID, x.Name });
                    table.ForeignKey(
                        name: "FK_Genres_TvRecommendations_TvRecommendationRecommendationID",
                        column: x => x.TvRecommendationRecommendationID,
                        principalTable: "TvRecommendations",
                        principalColumn: "RecommendationID");
                });

            migrationBuilder.CreateTable(
                name: "WatchListItems",
                columns: table => new
                {
                    WatchListID = table.Column<int>(type: "INTEGER", nullable: false),
                    MovieID = table.Column<int>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchListItems", x => new { x.WatchListID, x.MovieID });
                    table.ForeignKey(
                        name: "FK_WatchListItems_Movies_MovieID",
                        column: x => x.MovieID,
                        principalTable: "Movies",
                        principalColumn: "TMDBMovieID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchListItems_WatchLists_WatchListID",
                        column: x => x.WatchListID,
                        principalTable: "WatchLists",
                        principalColumn: "WatchListID",
                        onDelete: ReferentialAction.Cascade);
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
                    { 21, "c0c717a4f8fad3366520d47c702ab5ad", "Total Recall", 861 }
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
                name: "IX_CrossRefVideoTMDBMovies_TMDBMovieID",
                table: "CrossRefVideoTMDBMovies",
                column: "TMDBMovieID");

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTMDBMovies_VideoID",
                table: "CrossRefVideoTMDBMovies",
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
                name: "IX_Genres_TvRecommendationRecommendationID",
                table: "Genres",
                column: "TvRecommendationRecommendationID");

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
                name: "IX_RecommendationGenres_GenreID_Name",
                table: "RecommendationGenres",
                columns: new[] { "GenreID", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_TMDBMovieID_Id",
                table: "Recommendations",
                columns: new[] { "TMDBMovieID", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_TvShowID",
                table: "Seasons",
                column: "TvShowID");

            migrationBuilder.CreateIndex(
                name: "IX_TvRecommendations_TvShowID",
                table: "TvRecommendations",
                column: "TvShowID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoPlaces_VideoID",
                table: "VideoPlaces",
                column: "VideoID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchListItems_MovieID",
                table: "WatchListItems",
                column: "MovieID");

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
                name: "DuplicateFiles");

            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "ExampleHash");

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
                name: "VideoPlaces");

            migrationBuilder.DropTable(
                name: "WatchListItems");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "WatchLists");

            migrationBuilder.DropTable(
                name: "TvRecommendations");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "TvShows");
        }
    }
}
