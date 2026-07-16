using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThuHaiDuong.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlForAffiliate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "affiliate_links",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "affiliate_links");
        }
    }
}
