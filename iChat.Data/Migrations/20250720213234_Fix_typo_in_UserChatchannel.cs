using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fix_typo_in_UserChatchannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChannelIid",
                table: "UserChatChannels",
                newName: "ChannelId");

            migrationBuilder.RenameColumn(
                name: "UserIid",
                table: "UserChatChannels",
                newName: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "UserChatChannels",
                newName: "ChannelIid");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserChatChannels",
                newName: "UserIid");
        }
    }
}
