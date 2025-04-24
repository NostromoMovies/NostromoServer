using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddedMoreTvHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TvExampleHashes",
                keyColumn: "Id",
                keyValue: 1,
                column: "ED2K",
                value: "a413da8e3e3bb02237795b2dc9e06b8d");

            migrationBuilder.InsertData(
                table: "TvExampleHashes",
                columns: new[] { "Id", "ED2K", "EpisodeNumber", "SeasonNumber", "Title", "TvShowId" },
                values: new object[,]
                {
                    { 2, "ee4a746481ec4a6a909943562aefe86a", 2, 1, "The Blacklist", 46952 },
                    { 3, "a73c8cf075a960af6004a257432b2435", 3, 1, "The Blacklist", 46952 },
                    { 4, "ea85563f8f9c051cab70a0139c5118da", 4, 1, "The Blacklist", 46952 },
                    { 5, "c5f51c3dc5b4b45c68e428ccc062949f", 5, 1, "The Blacklist", 46952 },
                    { 6, "7203ced34b4989a4527457a4c564e2c1", 1, 2, "The Blacklist", 46952 },
                    { 7, "8accb9f07416005acdd4d4d9bc790295", 2, 2, "The Blacklist", 46952 },
                    { 8, "41da21faa145d66664535b5084240096", 3, 2, "The Blacklist", 46952 },
                    { 9, "2bda47a34c226363615c0355e001683b", 4, 2, "The Blacklist", 46952 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TvExampleHashes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TvExampleHashes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TvExampleHashes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TvExampleHashes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TvExampleHashes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TvExampleHashes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TvExampleHashes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TvExampleHashes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.UpdateData(
                table: "TvExampleHashes",
                keyColumn: "Id",
                keyValue: 1,
                column: "ED2K",
                value: "ee4a746481ec4a6a909943562aefe86a");
        }
    }
}
