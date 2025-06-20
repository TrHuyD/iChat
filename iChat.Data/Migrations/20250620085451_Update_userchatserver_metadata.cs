using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_userchatserver_metadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserChatServers_UserChatServers_UserChatServerUserId_UserCh~",
                table: "UserChatServers");

            migrationBuilder.DropIndex(
                name: "IX_UserChatServers_UserChatServerUserId_UserChatServerChatServ~",
                table: "UserChatServers");

            migrationBuilder.DropColumn(
                name: "UserChatServerChatServerId",
                table: "UserChatServers");

            migrationBuilder.DropColumn(
                name: "UserChatServerUserId",
                table: "UserChatServers");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "JoinedAt",
                table: "UserChatServers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<short>(
                name: "Order",
                table: "UserChatServers",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoinedAt",
                table: "UserChatServers");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "UserChatServers");

            migrationBuilder.AddColumn<long>(
                name: "UserChatServerChatServerId",
                table: "UserChatServers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserChatServerUserId",
                table: "UserChatServers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserChatServers_UserChatServerUserId_UserChatServerChatServ~",
                table: "UserChatServers",
                columns: new[] { "UserChatServerUserId", "UserChatServerChatServerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserChatServers_UserChatServers_UserChatServerUserId_UserCh~",
                table: "UserChatServers",
                columns: new[] { "UserChatServerUserId", "UserChatServerChatServerId" },
                principalTable: "UserChatServers",
                principalColumns: new[] { "UserId", "ChatServerId" });
        }
    }
}
