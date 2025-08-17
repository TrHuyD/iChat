using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Unique_Hash_in_Mediafile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_Hash",
                table: "MediaFiles");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Hash",
                table: "MediaFiles",
                column: "Hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_Hash",
                table: "MediaFiles");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Hash",
                table: "MediaFiles",
                column: "Hash",
                unique: true);
        }
    }
}
