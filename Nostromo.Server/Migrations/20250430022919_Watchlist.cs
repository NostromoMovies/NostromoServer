using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class Watchlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WatchListItems_Movies_MovieID",
                table: "WatchListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WatchLists_Users_UserID",
                table: "WatchLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WatchListItems",
                table: "WatchListItems");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "WatchLists",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ProfileID",
                table: "WatchLists",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TvShowID",
                table: "WatchListItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProfileID",
                table: "Collections",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Collections",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WatchListItems",
                table: "WatchListItems",
                columns: new[] { "WatchListID", "MovieID", "TvShowID" });

            migrationBuilder.CreateIndex(
                name: "IX_WatchLists_ProfileID",
                table: "WatchLists",
                column: "ProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchListItems_TvShowID",
                table: "WatchListItems",
                column: "TvShowID");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WatchListItem_MovieOrTvShow",
                table: "WatchListItems",
                sql: "(MovieID IS NOT NULL AND TvShowID IS NULL) OR (MovieID IS NULL AND TvShowID IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ProfileID",
                table: "Collections",
                column: "ProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_UserID",
                table: "Collections",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Profiles_ProfileID",
                table: "Collections",
                column: "ProfileID",
                principalTable: "Profiles",
                principalColumn: "ProfileID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Users_UserID",
                table: "Collections",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WatchListItems_Movies_MovieID",
                table: "WatchListItems",
                column: "MovieID",
                principalTable: "Movies",
                principalColumn: "TMDBMovieID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WatchListItems_TvShows_TvShowID",
                table: "WatchListItems",
                column: "TvShowID",
                principalTable: "TvShows",
                principalColumn: "TvShowID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WatchLists_Profiles_ProfileID",
                table: "WatchLists",
                column: "ProfileID",
                principalTable: "Profiles",
                principalColumn: "ProfileID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WatchLists_Users_UserID",
                table: "WatchLists",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Profiles_ProfileID",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Users_UserID",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_WatchListItems_Movies_MovieID",
                table: "WatchListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WatchListItems_TvShows_TvShowID",
                table: "WatchListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WatchLists_Profiles_ProfileID",
                table: "WatchLists");

            migrationBuilder.DropForeignKey(
                name: "FK_WatchLists_Users_UserID",
                table: "WatchLists");

            migrationBuilder.DropIndex(
                name: "IX_WatchLists_ProfileID",
                table: "WatchLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WatchListItems",
                table: "WatchListItems");

            migrationBuilder.DropIndex(
                name: "IX_WatchListItems_TvShowID",
                table: "WatchListItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WatchListItem_MovieOrTvShow",
                table: "WatchListItems");

            migrationBuilder.DropIndex(
                name: "IX_Collections_ProfileID",
                table: "Collections");

            migrationBuilder.DropIndex(
                name: "IX_Collections_UserID",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "ProfileID",
                table: "WatchLists");

            migrationBuilder.DropColumn(
                name: "TvShowID",
                table: "WatchListItems");

            migrationBuilder.DropColumn(
                name: "ProfileID",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Collections");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "WatchLists",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WatchListItems",
                table: "WatchListItems",
                columns: new[] { "WatchListID", "MovieID" });

            migrationBuilder.AddForeignKey(
                name: "FK_WatchListItems_Movies_MovieID",
                table: "WatchListItems",
                column: "MovieID",
                principalTable: "Movies",
                principalColumn: "TMDBMovieID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WatchLists_Users_UserID",
                table: "WatchLists",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
