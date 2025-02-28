using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ebook_svc.Migrations
{
    /// <inheritdoc />
    public partial class Edit2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "Users",
                newName: "ImageURL");

            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "Books",
                newName: "ImageURL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageURL",
                table: "Users",
                newName: "ImageData");

            migrationBuilder.RenameColumn(
                name: "ImageURL",
                table: "Books",
                newName: "ImageData");
        }
    }
}
