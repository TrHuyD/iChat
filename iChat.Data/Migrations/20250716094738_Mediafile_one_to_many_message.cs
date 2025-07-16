using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Mediafile_one_to_many_message : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_MediaFiles_MediaId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_MediaId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MediaId",
                table: "Messages",
                column: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_MediaFiles_MediaId",
                table: "Messages",
                column: "MediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_MediaFiles_MediaId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_MediaId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MediaId",
                table: "Messages",
                column: "MediaId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_MediaFiles_MediaId",
                table: "Messages",
                column: "MediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id");
        }
    }
}
