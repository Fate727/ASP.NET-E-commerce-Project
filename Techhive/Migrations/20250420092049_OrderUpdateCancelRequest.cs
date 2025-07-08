using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Techhive.Migrations
{
    /// <inheritdoc />
    public partial class OrderUpdateCancelRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CancelRequested",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelRequested",
                table: "Orders");
        }
    }
}
