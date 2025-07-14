using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class MessageAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BannedById",
                table: "ServerBans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastEditedAt",
                table: "Messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "message_audit_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<long>(type: "bigint", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_message_audit_logs_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_message_audit_logs_ChatChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "ChatChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_message_audit_logs_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerBans_BannedById",
                table: "ServerBans",
                column: "BannedById");

            migrationBuilder.CreateIndex(
                name: "IX_message_audit_logs_ActorUserId_Timestamp",
                table: "message_audit_logs",
                columns: new[] { "ActorUserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_message_audit_logs_ChannelId",
                table: "message_audit_logs",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_message_audit_logs_ChannelId_ActionType",
                table: "message_audit_logs",
                columns: new[] { "ChannelId", "ActionType" });

            migrationBuilder.CreateIndex(
                name: "IX_message_audit_logs_MessageId",
                table: "message_audit_logs",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerBans_AspNetUsers_BannedById",
                table: "ServerBans",
                column: "BannedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerBans_AspNetUsers_BannedById",
                table: "ServerBans");

            migrationBuilder.DropTable(
                name: "message_audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_ServerBans_BannedById",
                table: "ServerBans");

            migrationBuilder.DropColumn(
                name: "BannedById",
                table: "ServerBans");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Messages");
        }
    }
}
