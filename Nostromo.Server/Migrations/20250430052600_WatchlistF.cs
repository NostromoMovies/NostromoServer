using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class WatchlistF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WatchListItems",
                table: "WatchListItems");

            migrationBuilder.AlterColumn<int>(
                name: "TvShowID",
                table: "WatchListItems",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "MovieID",
                table: "WatchListItems",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "WatchListItemID",
                table: "WatchListItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WatchListItems",
                table: "WatchListItems",
                column: "WatchListItemID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchListItems_WatchListID",
                table: "WatchListItems",
                column: "WatchListID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WatchListItems",
                table: "WatchListItems");

            migrationBuilder.DropIndex(
                name: "IX_WatchListItems_WatchListID",
                table: "WatchListItems");

            migrationBuilder.DropColumn(
                name: "WatchListItemID",
                table: "WatchListItems");

            migrationBuilder.AlterColumn<int>(
                name: "TvShowID",
                table: "WatchListItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MovieID",
                table: "WatchListItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WatchListItems",
                table: "WatchListItems",
                columns: new[] { "WatchListID", "MovieID", "TvShowID" });
        }
    }
}
