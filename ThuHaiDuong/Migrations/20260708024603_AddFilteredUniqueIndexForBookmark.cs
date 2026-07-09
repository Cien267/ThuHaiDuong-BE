using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThuHaiDuong.Migrations
{
    /// <inheritdoc />
    public partial class AddFilteredUniqueIndexForBookmark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookmark_UserId_StoryId",
                table: "bookmarks");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmark_UserId_StoryId",
                table: "bookmarks",
                columns: new[] { "UserId", "StoryId" },
                unique: true,
                filter: "[DeletedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookmark_UserId_StoryId",
                table: "bookmarks");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmark_UserId_StoryId",
                table: "bookmarks",
                columns: new[] { "UserId", "StoryId" },
                unique: true);
        }
    }
}
