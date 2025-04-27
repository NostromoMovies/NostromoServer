using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class FixedTvRecommen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Certification",
                table: "TvRecommendations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "TvRecommendations",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Certification",
                table: "TvRecommendations");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "TvRecommendations");
        }
    }
}
