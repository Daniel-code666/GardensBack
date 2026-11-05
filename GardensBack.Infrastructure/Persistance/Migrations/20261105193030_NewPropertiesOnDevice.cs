using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class NewPropertiesOnDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "I2cAddress",
                schema: "Data",
                table: "Device",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "I2cSclPin",
                schema: "Data",
                table: "Device",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "I2cSdaPin",
                schema: "Data",
                table: "Device",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "I2cAddress",
                schema: "Data",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "I2cSclPin",
                schema: "Data",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "I2cSdaPin",
                schema: "Data",
                table: "Device");
        }
    }
}
