using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class FixedTv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_GenreTvRecommendation_GenresGenreID_GenresName",
                table: "GenreTvRecommendation",
                columns: new[] { "GenresGenreID", "GenresName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenreTvRecommendation");
        }
    }
}
