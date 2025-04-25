using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Syddjurs_Item_API.Migrations
{
    /// <inheritdoc />
    public partial class home11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryText",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryText",
                table: "Items");
        }
    }
}
