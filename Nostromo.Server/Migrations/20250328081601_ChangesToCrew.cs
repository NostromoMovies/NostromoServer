using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangesToCrew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TMDBTvEpisodeID",
                table: "MovieCrews",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TMDBTvShowID",
                table: "MovieCrews",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieCrews_TMDBTvEpisodeID",
                table: "MovieCrews",
                column: "TMDBTvEpisodeID");

            migrationBuilder.CreateIndex(
                name: "IX_MovieCrews_TMDBTvShowID",
                table: "MovieCrews",
                column: "TMDBTvShowID");

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

            migrationBuilder.AddForeignKey(
                name: "FK_MovieCrews_Episodes_TMDBTvEpisodeID",
                table: "MovieCrews",
                column: "TMDBTvEpisodeID",
                principalTable: "Episodes",
                principalColumn: "EpisodeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieCrews_TvShows_TMDBTvShowID",
                table: "MovieCrews",
                column: "TMDBTvShowID",
                principalTable: "TvShows",
                principalColumn: "TvShowID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieCasts_Episodes_TMDBTvEpisodeID",
                table: "MovieCasts");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieCrews_Episodes_TMDBTvEpisodeID",
                table: "MovieCrews");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieCrews_TvShows_TMDBTvShowID",
                table: "MovieCrews");

            migrationBuilder.DropIndex(
                name: "IX_MovieCrews_TMDBTvEpisodeID",
                table: "MovieCrews");

            migrationBuilder.DropIndex(
                name: "IX_MovieCrews_TMDBTvShowID",
                table: "MovieCrews");

            migrationBuilder.DropIndex(
                name: "IX_MovieCasts_TMDBTvEpisodeID",
                table: "MovieCasts");

            migrationBuilder.DropColumn(
                name: "TMDBTvEpisodeID",
                table: "MovieCrews");

            migrationBuilder.DropColumn(
                name: "TMDBTvShowID",
                table: "MovieCrews");
        }
    }
}
