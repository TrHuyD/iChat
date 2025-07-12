using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGinIndexIgnoreDeletedMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Messages",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "CASE WHEN NOT \"isDeleted\" THEN to_tsvector('english', coalesce(\"TextContent\", '')) ELSE NULL END",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector")
                .OldAnnotation("Npgsql:TsVectorConfig", "english")
                .OldAnnotation("Npgsql:TsVectorProperties", new[] { "TextContent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Messages",
                type: "tsvector",
                nullable: false,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "CASE WHEN NOT \"isDeleted\" THEN to_tsvector('english', coalesce(\"TextContent\", '')) ELSE NULL END")
                .Annotation("Npgsql:TsVectorConfig", "english")
                .Annotation("Npgsql:TsVectorProperties", new[] { "TextContent" });
        }
    }
}
