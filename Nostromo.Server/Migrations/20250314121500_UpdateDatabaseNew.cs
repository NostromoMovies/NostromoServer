using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_MediaType_MediaTypeMediaTmDBID",
                table: "Episodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Movies_MediaType_MediaTypeMediaTmDBID",
                table: "Movies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MediaType",
                table: "MediaType");

            migrationBuilder.RenameTable(
                name: "MediaType",
                newName: "MediaTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MediaTypes",
                table: "MediaTypes",
                column: "MediaTmDBID");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_MediaTypes_MediaTypeMediaTmDBID",
                table: "Episodes",
                column: "MediaTypeMediaTmDBID",
                principalTable: "MediaTypes",
                principalColumn: "MediaTmDBID",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_Episodes_MediaTypes_MediaTypeMediaTmDBID",
                table: "Episodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Movies_MediaTypes_MediaTypeMediaTmDBID",
                table: "Movies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MediaTypes",
                table: "MediaTypes");

            migrationBuilder.RenameTable(
                name: "MediaTypes",
                newName: "MediaType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MediaType",
                table: "MediaType",
                column: "MediaTmDBID");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_MediaType_MediaTypeMediaTmDBID",
                table: "Episodes",
                column: "MediaTypeMediaTmDBID",
                principalTable: "MediaType",
                principalColumn: "MediaTmDBID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_MediaType_MediaTypeMediaTmDBID",
                table: "Movies",
                column: "MediaTypeMediaTmDBID",
                principalTable: "MediaType",
                principalColumn: "MediaTmDBID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
