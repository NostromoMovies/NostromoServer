using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class Crea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 3,
                column: "ED2K",
                value: "57a4a50f53149b9b0c1e1db3e66d2e9c");

            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 4,
                column: "TmdbId",
                value: 46952);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 3,
                column: "ED2K",
                value: "b33d9c30eb480eca99e82dbbab3aad0e");

            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 4,
                column: "TmdbId",
                value: 203);
        }
    }
}
