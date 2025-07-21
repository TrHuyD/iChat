using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Change_bucket_index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_BucketId_Id",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_SenderId_Timestamp",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_Timestamp",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "last_seen",
                table: "UserChatServers");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_BucketId_Id",
                table: "Messages",
                columns: new[] { "ChannelId", "BucketId", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_SenderId_Timestamp",
                table: "Messages",
                columns: new[] { "ChannelId", "SenderId", "Timestamp" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_Timestamp",
                table: "Messages",
                columns: new[] { "ChannelId", "Timestamp" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_BucketId_Id",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_SenderId_Timestamp",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_Timestamp",
                table: "Messages");

            migrationBuilder.AddColumn<long>(
                name: "last_seen",
                table: "UserChatServers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId",
                table: "Messages",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_BucketId_Id",
                table: "Messages",
                columns: new[] { "ChannelId", "BucketId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_SenderId_Timestamp",
                table: "Messages",
                columns: new[] { "ChannelId", "SenderId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_Timestamp",
                table: "Messages",
                columns: new[] { "ChannelId", "Timestamp" });
        }
    }
}
