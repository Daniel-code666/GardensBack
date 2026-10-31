using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddFirmwareUpdateConfirmations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeviceConfirmedAt",
                schema: "Data",
                table: "DeviceFirmwareUpdate",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InventoryConfirmedAt",
                schema: "Data",
                table: "DeviceFirmwareUpdate",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceConfirmedAt",
                schema: "Data",
                table: "DeviceFirmwareUpdate");

            migrationBuilder.DropColumn(
                name: "InventoryConfirmedAt",
                schema: "Data",
                table: "DeviceFirmwareUpdate");
        }
    }
}
