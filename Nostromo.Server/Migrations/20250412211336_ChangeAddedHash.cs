using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAddedHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 2,
                column: "ED2K",
                value: "ee4a746481ec4a6a909943562aefe86a");

            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 4,
                column: "ED2K",
                value: "a413da8e3e3bb02237795b2dc9e06b8d");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 2,
                column: "ED2K",
                value: "da1a506c0ee1fe6c46ec64fd57faa924");

            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 4,
                column: "ED2K",
                value: "4f6eb190545f122d81ba9316d472f4b5");
        }
    }
}
