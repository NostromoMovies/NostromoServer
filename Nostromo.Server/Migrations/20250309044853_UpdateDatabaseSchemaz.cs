using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseSchemaz : Migration
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

            migrationBuilder.AddColumn<int>(
                name: "MediaTypeMediaTmDBID",
                table: "Episodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MediaType",
                columns: table => new
                {
                    MediaTmDBID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MediaTypeName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaType", x => x.MediaTmDBID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movies_MediaTypeMediaTmDBID",
                table: "Movies",
                column: "MediaTypeMediaTmDBID");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_MediaTypeMediaTmDBID",
                table: "Episodes",
                column: "MediaTypeMediaTmDBID");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_MediaType_MediaTypeMediaTmDBID",
                table: "Episodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Movies_MediaType_MediaTypeMediaTmDBID",
                table: "Movies");

            migrationBuilder.DropTable(
                name: "MediaType");

            migrationBuilder.DropIndex(
                name: "IX_Movies_MediaTypeMediaTmDBID",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Episodes_MediaTypeMediaTmDBID",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "MediaTypeMediaTmDBID",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "MediaTypeMediaTmDBID",
                table: "Episodes");
        }
    }
}
