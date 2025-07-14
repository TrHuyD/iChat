using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviousContentForAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_message_audit_logs_AspNetUsers_ActorUserId",
                table: "message_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_message_audit_logs_ChatChannels_ChannelId",
                table: "message_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_message_audit_logs_Messages_MessageId",
                table: "message_audit_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_message_audit_logs",
                table: "message_audit_logs");

            migrationBuilder.RenameTable(
                name: "message_audit_logs",
                newName: "MessageAuditLogs");

            migrationBuilder.RenameIndex(
                name: "IX_message_audit_logs_MessageId",
                table: "MessageAuditLogs",
                newName: "IX_MessageAuditLogs_MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_message_audit_logs_ChannelId_ActionType",
                table: "MessageAuditLogs",
                newName: "IX_MessageAuditLogs_ChannelId_ActionType");

            migrationBuilder.RenameIndex(
                name: "IX_message_audit_logs_ChannelId",
                table: "MessageAuditLogs",
                newName: "IX_MessageAuditLogs_ChannelId");

            migrationBuilder.RenameIndex(
                name: "IX_message_audit_logs_ActorUserId_Timestamp",
                table: "MessageAuditLogs",
                newName: "IX_MessageAuditLogs_ActorUserId_Timestamp");

            migrationBuilder.AlterColumn<short>(
                name: "ActionType",
                table: "MessageAuditLogs",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "PreviousContent",
                table: "MessageAuditLogs",
                type: "character varying(40000)",
                maxLength: 40000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageAuditLogs",
                table: "MessageAuditLogs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAuditLogs_AspNetUsers_ActorUserId",
                table: "MessageAuditLogs",
                column: "ActorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAuditLogs_ChatChannels_ChannelId",
                table: "MessageAuditLogs",
                column: "ChannelId",
                principalTable: "ChatChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAuditLogs_Messages_MessageId",
                table: "MessageAuditLogs",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageAuditLogs_AspNetUsers_ActorUserId",
                table: "MessageAuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageAuditLogs_ChatChannels_ChannelId",
                table: "MessageAuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageAuditLogs_Messages_MessageId",
                table: "MessageAuditLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageAuditLogs",
                table: "MessageAuditLogs");

            migrationBuilder.DropColumn(
                name: "PreviousContent",
                table: "MessageAuditLogs");

            migrationBuilder.RenameTable(
                name: "MessageAuditLogs",
                newName: "message_audit_logs");

            migrationBuilder.RenameIndex(
                name: "IX_MessageAuditLogs_MessageId",
                table: "message_audit_logs",
                newName: "IX_message_audit_logs_MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageAuditLogs_ChannelId_ActionType",
                table: "message_audit_logs",
                newName: "IX_message_audit_logs_ChannelId_ActionType");

            migrationBuilder.RenameIndex(
                name: "IX_MessageAuditLogs_ChannelId",
                table: "message_audit_logs",
                newName: "IX_message_audit_logs_ChannelId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageAuditLogs_ActorUserId_Timestamp",
                table: "message_audit_logs",
                newName: "IX_message_audit_logs_ActorUserId_Timestamp");

            migrationBuilder.AlterColumn<int>(
                name: "ActionType",
                table: "message_audit_logs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_message_audit_logs",
                table: "message_audit_logs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_message_audit_logs_AspNetUsers_ActorUserId",
                table: "message_audit_logs",
                column: "ActorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_message_audit_logs_ChatChannels_ChannelId",
                table: "message_audit_logs",
                column: "ChannelId",
                principalTable: "ChatChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_message_audit_logs_Messages_MessageId",
                table: "message_audit_logs",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
