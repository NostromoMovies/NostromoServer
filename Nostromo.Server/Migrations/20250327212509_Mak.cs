using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class Mak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieCasts_Episodes_TMDBTvEpisodeID",
                table: "MovieCasts");

            migrationBuilder.DropIndex(
                name: "IX_MovieCasts_TMDBTvEpisodeID",
                table: "MovieCasts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MovieCasts_TMDBTvEpisodeID",
                table: "MovieCasts",
                column: "TMDBTvEpisodeID");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieCasts_Episodes_TMDBTvEpisodeID",
                table: "MovieCasts",
                column: "TMDBTvEpisodeID",
                principalTable: "Episodes",
                principalColumn: "EpisodeID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
