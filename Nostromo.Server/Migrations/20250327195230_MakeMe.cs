using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class MakeMe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MediaTypeMediaTmDBID",
                table: "Movies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_MediaTypeMediaTmDBID",
                table: "Movies",
                column: "MediaTypeMediaTmDBID");

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_MediaTypes_MediaTypeMediaTmDBID",
                table: "Movies",
                column: "MediaTypeMediaTmDBID",
                principalTable: "MediaTypes",
                principalColumn: "MediaTmDBID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_MediaTypes_MediaTypeMediaTmDBID",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_MediaTypeMediaTmDBID",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "MediaTypeMediaTmDBID",
                table: "Movies");
        }
    }
}
