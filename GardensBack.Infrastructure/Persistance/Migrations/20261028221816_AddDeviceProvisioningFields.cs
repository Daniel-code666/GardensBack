using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceProvisioningFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BootstrapRequestId",
                schema: "Data",
                table: "DeviceClaim",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProvisioningTokenConsumedAt",
                schema: "Data",
                table: "DeviceClaim",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProvisioningTokenExpiresAt",
                schema: "Data",
                table: "DeviceClaim",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvisioningTokenHash",
                schema: "Data",
                table: "DeviceClaim",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseAuthenticationTag",
                schema: "Data",
                table: "DeviceClaim",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceClaim_BootstrapRequestId",
                schema: "Data",
                table: "DeviceClaim",
                column: "BootstrapRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceClaim_ProvisioningTokenHash",
                schema: "Data",
                table: "DeviceClaim",
                column: "ProvisioningTokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceClaim_BootstrapRequestId",
                schema: "Data",
                table: "DeviceClaim");

            migrationBuilder.DropIndex(
                name: "IX_DeviceClaim_ProvisioningTokenHash",
                schema: "Data",
                table: "DeviceClaim");

            migrationBuilder.DropColumn(
                name: "BootstrapRequestId",
                schema: "Data",
                table: "DeviceClaim");

            migrationBuilder.DropColumn(
                name: "ProvisioningTokenConsumedAt",
                schema: "Data",
                table: "DeviceClaim");

            migrationBuilder.DropColumn(
                name: "ProvisioningTokenExpiresAt",
                schema: "Data",
                table: "DeviceClaim");

            migrationBuilder.DropColumn(
                name: "ProvisioningTokenHash",
                schema: "Data",
                table: "DeviceClaim");

            migrationBuilder.DropColumn(
                name: "ResponseAuthenticationTag",
                schema: "Data",
                table: "DeviceClaim");
        }
    }
}
