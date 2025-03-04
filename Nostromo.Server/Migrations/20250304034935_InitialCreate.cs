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
                    GenreID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.GenreID);
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
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "MovieGenre",
                columns: table => new
                {
                    MovieID = table.Column<int>(type: "INTEGER", nullable: false),
                    GenreID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieGenre", x => new { x.GenreID, x.MovieID });
                    table.ForeignKey(
                        name: "FK_MovieGenre_Genres_GenreID",
                        column: x => x.GenreID,
                        principalTable: "Genres",
                        principalColumn: "GenreID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieGenre_Movies_MovieID",
                        column: x => x.MovieID,
                        principalTable: "Movies",
                        principalColumn: "TMDBMovieID",
                        onDelete: ReferentialAction.Cascade);
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
                    GenreIds = table.Column<string>(type: "TEXT", nullable: false),
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

            migrationBuilder.InsertData(
                table: "ExampleHash",
                columns: new[] { "Id", "ED2K", "Title", "TmdbId" },
                values: new object[,]
                {
                    { 1, "5d886780825db91bbc390f10f1b6c95c", "Alien", 348 },
                    { 2, "da1a506c0ee1fe6c46ec64fd57faa924", "Aliens", 679 },
                    { 3, "b33d9c30eb480eca99e82dbbab3aad0e", "Alien 3", 8077 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthTokens_UserId",
                table: "AuthTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTMDBMovies_TMDBMovieID",
                table: "CrossRefVideoTMDBMovies",
                column: "TMDBMovieID");

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTMDBMovies_VideoID",
                table: "CrossRefVideoTMDBMovies",
                column: "VideoID");

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
                name: "IX_MovieGenre_MovieID",
                table: "MovieGenre",
                column: "MovieID");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_TMDBMovieID_Id",
                table: "Recommendations",
                columns: new[] { "TMDBMovieID", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoPlaces_VideoID",
                table: "VideoPlaces",
                column: "VideoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthTokens");

            migrationBuilder.DropTable(
                name: "CrossRefVideoTMDBMovies");

            migrationBuilder.DropTable(
                name: "DuplicateFiles");

            migrationBuilder.DropTable(
                name: "ExampleHash");

            migrationBuilder.DropTable(
                name: "ImportFolders");

            migrationBuilder.DropTable(
                name: "MovieCasts");

            migrationBuilder.DropTable(
                name: "MovieCrews");

            migrationBuilder.DropTable(
                name: "MovieGenre");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "VideoPlaces");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Videos");
        }
    }
}
