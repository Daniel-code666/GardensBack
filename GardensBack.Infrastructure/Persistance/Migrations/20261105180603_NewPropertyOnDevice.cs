using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class NewPropertyOnDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomName",
                schema: "Data",
                table: "Device",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomName",
                schema: "Data",
                table: "Device");
        }
    }
}
