using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_MediaTypes_MediaTypeMediaTmDBID",
                table: "Episodes");

            migrationBuilder.DropIndex(
                name: "IX_Episodes_MediaTypeMediaTmDBID",
                table: "Episodes");

            migrationBuilder.RenameColumn(
                name: "MediaTypeMediaTmDBID",
                table: "Episodes",
                newName: "EpisodeNumber");

            migrationBuilder.AddColumn<int>(
                name: "MediaTypeMediaTmDBID",
                table: "TvShows",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeasonNumber",
                table: "Seasons",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EpisodeNo",
                table: "ExampleHash",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonNo",
                table: "ExampleHash",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EpisodeName",
                table: "Episodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EpisodeNo", "SeasonNo" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EpisodeNo", "SeasonNo" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EpisodeNo", "SeasonNo" },
                values: new object[] { null, null });

            migrationBuilder.InsertData(
                table: "ExampleHash",
                columns: new[] { "Id", "ED2K", "EpisodeNo", "SeasonNo", "Title", "TmdbId" },
                values: new object[] { 4, "", 1, 1, "The Blacklist", 46952 });

            migrationBuilder.CreateIndex(
                name: "IX_TvShows_MediaTypeMediaTmDBID",
                table: "TvShows",
                column: "MediaTypeMediaTmDBID");

            migrationBuilder.AddForeignKey(
                name: "FK_TvShows_MediaTypes_MediaTypeMediaTmDBID",
                table: "TvShows",
                column: "MediaTypeMediaTmDBID",
                principalTable: "MediaTypes",
                principalColumn: "MediaTmDBID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TvShows_MediaTypes_MediaTypeMediaTmDBID",
                table: "TvShows");

            migrationBuilder.DropIndex(
                name: "IX_TvShows_MediaTypeMediaTmDBID",
                table: "TvShows");

            migrationBuilder.DeleteData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "MediaTypeMediaTmDBID",
                table: "TvShows");

            migrationBuilder.DropColumn(
                name: "SeasonNumber",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "EpisodeNo",
                table: "ExampleHash");

            migrationBuilder.DropColumn(
                name: "SeasonNo",
                table: "ExampleHash");

            migrationBuilder.DropColumn(
                name: "EpisodeName",
                table: "Episodes");

            migrationBuilder.RenameColumn(
                name: "EpisodeNumber",
                table: "Episodes",
                newName: "MediaTypeMediaTmDBID");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_MediaTypeMediaTmDBID",
                table: "Episodes",
                column: "MediaTypeMediaTmDBID");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_MediaTypes_MediaTypeMediaTmDBID",
                table: "Episodes",
                column: "MediaTypeMediaTmDBID",
                principalTable: "MediaTypes",
                principalColumn: "MediaTmDBID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
