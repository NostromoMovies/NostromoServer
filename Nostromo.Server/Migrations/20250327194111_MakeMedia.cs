using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class MakeMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MediaTypeMediaTmDBID",
                table: "TvShows",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.DropColumn(
                name: "MediaTypeMediaTmDBID",
                table: "TvShows");
        }
    }
}
