using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTvShowSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "MovieCasts");

            migrationBuilder.AlterColumn<int>(
                name: "TMDBMovieID",
                table: "MovieCasts",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "TMDBTvEpisodeID",
                table: "MovieCasts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TMDBTvShowID",
                table: "MovieCasts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TmdbCastMemberId",
                table: "MovieCasts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieCasts_TMDBTvEpisodeID",
                table: "MovieCasts",
                column: "TMDBTvEpisodeID");

            migrationBuilder.CreateIndex(
                name: "IX_MovieCasts_TMDBTvShowID",
                table: "MovieCasts",
                column: "TMDBTvShowID");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieCasts_Episodes_TMDBTvEpisodeID",
                table: "MovieCasts",
                column: "TMDBTvEpisodeID",
                principalTable: "Episodes",
                principalColumn: "EpisodeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieCasts_TvShows_TMDBTvShowID",
                table: "MovieCasts",
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
                name: "FK_MovieCasts_TvShows_TMDBTvShowID",
                table: "MovieCasts");

            migrationBuilder.DropIndex(
                name: "IX_MovieCasts_TMDBTvEpisodeID",
                table: "MovieCasts");

            migrationBuilder.DropIndex(
                name: "IX_MovieCasts_TMDBTvShowID",
                table: "MovieCasts");

            migrationBuilder.DropColumn(
                name: "TMDBTvEpisodeID",
                table: "MovieCasts");

            migrationBuilder.DropColumn(
                name: "TMDBTvShowID",
                table: "MovieCasts");

            migrationBuilder.DropColumn(
                name: "TmdbCastMemberId",
                table: "MovieCasts");

            migrationBuilder.AlterColumn<int>(
                name: "TMDBMovieID",
                table: "MovieCasts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "MovieCasts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
