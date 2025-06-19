using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Order_to_chatchannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Order",
                table: "ChatChannels",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "ChatChannels");
        }
    }
}
