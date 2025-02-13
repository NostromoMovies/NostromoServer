using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExampleHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ExampleHash",
                columns: new[] { "Id", "ED2K", "Title", "TmdbId" },
                values: new object[,]
                {
                    { 2, "da1a506c0ee1fe6c46ec64fd57faa924", "Aliens", 679 },
                    { 3, "b33d9c30eb480eca99e82dbbab3aad0e", "Alien 3", 8077 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
