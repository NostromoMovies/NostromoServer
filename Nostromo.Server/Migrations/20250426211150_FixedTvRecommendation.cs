using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class FixedTvRecommendation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TvRecommendations_TvShows_TvShowID",
                table: "TvRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_TvRecommendations_TvShowID",
                table: "TvRecommendations");

            migrationBuilder.DropColumn(
                name: "TvShowID",
                table: "TvRecommendations");

            migrationBuilder.CreateIndex(
                name: "IX_TvRecommendations_ShowId",
                table: "TvRecommendations",
                column: "ShowId");

            migrationBuilder.AddForeignKey(
                name: "FK_TvRecommendations_TvShows_ShowId",
                table: "TvRecommendations",
                column: "ShowId",
                principalTable: "TvShows",
                principalColumn: "TvShowID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TvRecommendations_TvShows_ShowId",
                table: "TvRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_TvRecommendations_ShowId",
                table: "TvRecommendations");

            migrationBuilder.AddColumn<int>(
                name: "TvShowID",
                table: "TvRecommendations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TvRecommendations_TvShowID",
                table: "TvRecommendations",
                column: "TvShowID");

            migrationBuilder.AddForeignKey(
                name: "FK_TvRecommendations_TvShows_TvShowID",
                table: "TvRecommendations",
                column: "TvShowID",
                principalTable: "TvShows",
                principalColumn: "TvShowID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
