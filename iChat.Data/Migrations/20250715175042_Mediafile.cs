using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Mediafile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaContent",
                table: "Messages");

            migrationBuilder.AddColumn<int>(
                name: "MediaId",
                table: "Messages",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Messages",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "CASE WHEN NOT \"isDeleted\" THEN to_tsvector('english', coalesce(\"TextContent\", '')) ELSE NULL END",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "CASE WHEN NOT \"isDeleted\" THEN to_tsvector('english', coalesce(\"TextContent\", '')) ELSE NULL END",
                oldStored: true);

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    SizeBytes = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploaderUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MediaId",
                table: "Messages",
                column: "MediaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Hash",
                table: "MediaFiles",
                column: "Hash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_MediaFiles_MediaId",
                table: "Messages",
                column: "MediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_MediaFiles_MediaId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "MediaFiles");

            migrationBuilder.DropIndex(
                name: "IX_Messages_MediaId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "MediaContent",
                table: "Messages",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Messages",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "CASE WHEN NOT \"isDeleted\" THEN to_tsvector('english', coalesce(\"TextContent\", '')) ELSE NULL END",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true,
                oldComputedColumnSql: "CASE WHEN NOT \"isDeleted\" THEN to_tsvector('english', coalesce(\"TextContent\", '')) ELSE NULL END",
                oldStored: true);
        }
    }
}
