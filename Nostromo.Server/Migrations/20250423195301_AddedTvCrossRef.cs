using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostromo.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddedTvCrossRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrossRefVideoTvEpisodes",
                columns: table => new
                {
                    CrossRefVideoTvEpisodeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoID = table.Column<int>(type: "INTEGER", nullable: false),
                    TvEpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrossRefVideoTvEpisodes", x => x.CrossRefVideoTvEpisodeID);
                    table.ForeignKey(
                        name: "FK_CrossRefVideoTvEpisodes_Episodes_TvEpisodeId",
                        column: x => x.TvEpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "EpisodeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrossRefVideoTvEpisodes_Videos_VideoID",
                        column: x => x.VideoID,
                        principalTable: "Videos",
                        principalColumn: "VideoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTvEpisodes_TvEpisodeId",
                table: "CrossRefVideoTvEpisodes",
                column: "TvEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CrossRefVideoTvEpisodes_VideoID",
                table: "CrossRefVideoTvEpisodes",
                column: "VideoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrossRefVideoTvEpisodes");
        }
    }
}
