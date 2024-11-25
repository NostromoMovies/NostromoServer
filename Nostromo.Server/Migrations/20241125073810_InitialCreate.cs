using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                name: "MovieCasts",
                columns: table => new
                {
                    TMDBMovieCastID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TMDBPersonID = table.Column<int>(type: "INTEGER", nullable: false),
                    TMDBCreditID = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacterName = table.Column<string>(type: "TEXT", nullable: false),
                    Ordering = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieCasts", x => x.TMDBMovieCastID);
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
                    EnglishName = table.Column<string>(type: "TEXT", nullable: false),
                    EnglishBio = table.Column<string>(type: "TEXT", nullable: false),
                    Aliases = table.Column<string>(type: "TEXT", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRestricted = table.Column<bool>(type: "INTEGER", nullable: false),
                    BirthDay = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "CrossRefVideoTMDBMovies",
                columns: table => new
                {
                    CrossRefVideoTMDBMovieID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoID = table.Column<int>(type: "INTEGER", nullable: false),
                    TMDBMovieID = table.Column<int>(type: "INTEGER", nullable: false)
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
                values: new object[] { 1, "5d886780825db91bbc390f10f1b6c95c", "Alien", 348 });

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTMDBMovies_TMDBMovieID",
                table: "CrossRefVideoTMDBMovies",
                column: "TMDBMovieID");

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTMDBMovies_VideoID",
                table: "CrossRefVideoTMDBMovies",
                column: "VideoID");

            migrationBuilder.CreateIndex(
                name: "IX_MovieGenre_MovieID",
                table: "MovieGenre",
                column: "MovieID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoPlaces_VideoID",
                table: "VideoPlaces",
                column: "VideoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "MovieGenre");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VideoPlaces");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Videos");
        }
    }
}
