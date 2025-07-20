using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_UserChatchannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserChatChannels",
                columns: table => new
                {
                    UserIid = table.Column<long>(type: "bigint", nullable: false),
                    ChannelIid = table.Column<long>(type: "bigint", nullable: false),
                    LastSeenMessage = table.Column<long>(type: "bigint", nullable: false),
                    NotificationCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChatChannels", x => new { x.UserIid, x.ChannelIid });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserChatChannels");
        }
    }
}
