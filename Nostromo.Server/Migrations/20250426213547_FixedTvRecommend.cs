using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class FixedTvRecommend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "TvRecommendations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "TvRecommendations",
                type: "TEXT",
                nullable: true);
        }
    }
}
