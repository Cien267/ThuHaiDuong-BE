using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThuHaiDuong.Migrations
{
    /// <inheritdoc />
    public partial class AddFilteredUniqueIndexForReadingHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadingHistory_UserId_ChapterId",
                table: "reading_histories");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingHistory_UserId_ChapterId",
                table: "reading_histories",
                columns: new[] { "UserId", "ChapterId" },
                unique: true,
                filter: "[DeletedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadingHistory_UserId_ChapterId",
                table: "reading_histories");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingHistory_UserId_ChapterId",
                table: "reading_histories",
                columns: new[] { "UserId", "ChapterId" },
                unique: true);
        }
    }
}
