using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class Creater : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ED2K", "TmdbId" },
                values: new object[] { "4f6eb190545f122d81ba9316d472f4b5", 203 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ExampleHash",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ED2K", "TmdbId" },
                values: new object[] { "", 46952 });
        }
    }
}
