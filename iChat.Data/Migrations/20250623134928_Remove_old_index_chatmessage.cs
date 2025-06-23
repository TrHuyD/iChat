using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Remove_old_index_chatmessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_BucketId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_BucketId_Timestamp",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_Id",
                table: "Messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_BucketId",
                table: "Messages",
                columns: new[] { "ChannelId", "BucketId" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_BucketId_Timestamp",
                table: "Messages",
                columns: new[] { "ChannelId", "BucketId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_Id",
                table: "Messages",
                columns: new[] { "ChannelId", "Id" });
        }
    }
}
