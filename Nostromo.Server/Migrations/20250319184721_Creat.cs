using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class Creat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Airdate",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EpisodeCount",
                table: "Seasons",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Overview",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PosterPath",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "VoteAverage",
                table: "Seasons",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "seasonName",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "CastId",
                table: "MovieCasts",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "TvRecommendationRecommendationID",
                table: "Genres",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Airdate",
                table: "Episodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Overview",
                table: "Episodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Runtime",
                table: "Episodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SeasonNumber",
                table: "Episodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StillPath",
                table: "Episodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "VoteAverage",
                table: "Episodes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "VoteCount",
                table: "Episodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_Genres_TvRecommendationRecommendationID",
                table: "Genres",
                column: "TvRecommendationRecommendationID");

            migrationBuilder.CreateIndex(
                name: "IX_TvRecommendations_TvShowID",
                table: "TvRecommendations",
                column: "TvShowID");

            migrationBuilder.AddForeignKey(
                name: "FK_Genres_TvRecommendations_TvRecommendationRecommendationID",
                table: "Genres",
                column: "TvRecommendationRecommendationID",
                principalTable: "TvRecommendations",
                principalColumn: "RecommendationID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Genres_TvRecommendations_TvRecommendationRecommendationID",
                table: "Genres");

            migrationBuilder.DropTable(
                name: "TvRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_Genres_TvRecommendationRecommendationID",
                table: "Genres");

            migrationBuilder.DropColumn(
                name: "Airdate",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "EpisodeCount",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "Overview",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "PosterPath",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "VoteAverage",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "seasonName",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "TvRecommendationRecommendationID",
                table: "Genres");

            migrationBuilder.DropColumn(
                name: "Airdate",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "Overview",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "Runtime",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "SeasonNumber",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "StillPath",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "VoteAverage",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "VoteCount",
                table: "Episodes");

            migrationBuilder.AlterColumn<int>(
                name: "CastId",
                table: "MovieCasts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
