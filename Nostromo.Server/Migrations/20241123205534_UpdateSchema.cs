using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportFolders",
                columns: table => new
                {
                    ImportFolderID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImportFolderType = table.Column<int>(type: "INTEGER", nullable: false),
                    FolderLocation = table.Column<string>(type: "TEXT", nullable: false),
                    IsDropSource = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDropDestination = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsWatched = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportFolders", x => x.ImportFolderID);
                });

            migrationBuilder.CreateTable(
                name: "TMDBPersons",
                columns: table => new
                {
                    TMDBPersonID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TMDBID = table.Column<int>(type: "INTEGER", nullable: false),
                    EnglishName = table.Column<string>(type: "TEXT", nullable: false),
                    EnglishBio = table.Column<string>(type: "TEXT", nullable: false),
                    Alias = table.Column<string>(type: "TEXT", nullable: false),
                    Gender = table.Column<string>(type: "TEXT", nullable: false),
                    IsRestricted = table.Column<bool>(type: "INTEGER", nullable: false),
                    BirthDay = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlaceOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TMDBPersons", x => x.TMDBPersonID);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    VideoID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    CRC32 = table.Column<string>(type: "TEXT", nullable: false),
                    SHA1 = table.Column<string>(type: "TEXT", nullable: false),
                    MD5 = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.VideoID);
                });

            migrationBuilder.CreateTable(
                name: "TMDBMovieCasts",
                columns: table => new
                {
                    TMDBMovieCastID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TMDBMovieID = table.Column<int>(type: "INTEGER", nullable: false),
                    TMDBPersonID = table.Column<int>(type: "INTEGER", nullable: false),
                    TMDBCreditID = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacterName = table.Column<string>(type: "TEXT", nullable: false),
                    Ordering = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TMDBMovieCasts", x => x.TMDBMovieCastID);
                    table.ForeignKey(
                        name: "FK_TMDBMovieCasts_Movies_TMDBMovieID",
                        column: x => x.TMDBMovieID,
                        principalTable: "Movies",
                        principalColumn: "MovieID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TMDBMovieCasts_TMDBPersons_TMDBPersonID",
                        column: x => x.TMDBPersonID,
                        principalTable: "TMDBPersons",
                        principalColumn: "TMDBPersonID",
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
                        principalColumn: "MovieID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrossRefVideoTMDBMovies_Videos_VideoID",
                        column: x => x.VideoID,
                        principalTable: "Videos",
                        principalColumn: "VideoID",
                        onDelete: ReferentialAction.Cascade);
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
                    ImportFolderType = table.Column<int>(type: "INTEGER", nullable: false),
                    VideoID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateFiles", x => x.DuplicateFileID);
                    table.ForeignKey(
                        name: "FK_DuplicateFiles_Videos_VideoID",
                        column: x => x.VideoID,
                        principalTable: "Videos",
                        principalColumn: "VideoID");
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

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTMDBMovies_TMDBMovieID",
                table: "CrossRefVideoTMDBMovies",
                column: "TMDBMovieID");

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTMDBMovies_VideoID",
                table: "CrossRefVideoTMDBMovies",
                column: "VideoID");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateFiles_VideoID",
                table: "DuplicateFiles",
                column: "VideoID");

            migrationBuilder.CreateIndex(
                name: "IX_TMDBMovieCasts_TMDBMovieID",
                table: "TMDBMovieCasts",
                column: "TMDBMovieID");

            migrationBuilder.CreateIndex(
                name: "IX_TMDBMovieCasts_TMDBPersonID",
                table: "TMDBMovieCasts",
                column: "TMDBPersonID");

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
                name: "ImportFolders");

            migrationBuilder.DropTable(
                name: "TMDBMovieCasts");

            migrationBuilder.DropTable(
                name: "VideoPlaces");

            migrationBuilder.DropTable(
                name: "TMDBPersons");

            migrationBuilder.DropTable(
                name: "Videos");
        }
    }
}
