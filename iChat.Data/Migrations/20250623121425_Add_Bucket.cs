using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Bucket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""Messages"";");
            // Step 1: Add BucketId column to Messages
            migrationBuilder.AddColumn<int>(
                name: "BucketId",
                table: "Messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Step 2: Create Buckets table (no FK yet)
            migrationBuilder.CreateTable(
                name: "Buckets",
                columns: table => new
                {
                    ChannelId = table.Column<long>(type: "bigint", nullable: false),
                    BucketId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buckets", x => new { x.ChannelId, x.BucketId });
                    table.ForeignKey(
                        name: "FK_Buckets_ChatChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "ChatChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Step 3: Set default bucket ID in Messages (already defaulted, but safe to repeat)
            migrationBuilder.Sql(@"
        UPDATE ""Messages""
        SET ""BucketId"" = 0
        WHERE ""BucketId"" IS NULL;
    ");

            // Step 4: Insert bucket 0 for all existing channels
            migrationBuilder.Sql(@"
        INSERT INTO ""Buckets"" (""ChannelId"", ""BucketId"", ""CreatedAt"")
        SELECT DISTINCT ""ChannelId"", 0, NOW()
        FROM ""Messages""
        WHERE ""ChannelId"" IS NOT NULL;
    ");

            // Step 5: Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_BucketId",
                table: "Messages",
                columns: new[] { "ChannelId", "BucketId" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_BucketId_Id",
                table: "Messages",
                columns: new[] { "ChannelId", "BucketId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_BucketId_Timestamp",
                table: "Messages",
                columns: new[] { "ChannelId", "BucketId", "Timestamp" });

            // Step 6: Add FK constraints now that required data exists
            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Buckets_ChannelId_BucketId",
                table: "Messages",
                columns: new[] { "ChannelId", "BucketId" },
                principalTable: "Buckets",
                principalColumns: new[] { "ChannelId", "BucketId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChatChannels_ChannelId",
                table: "Messages",
                column: "ChannelId",
                principalTable: "ChatChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Buckets_ChannelId_BucketId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChatChannels_ChannelId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "Buckets");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_BucketId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_BucketId_Id",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_BucketId_Timestamp",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "BucketId",
                table: "Messages");
        }
    }
}
