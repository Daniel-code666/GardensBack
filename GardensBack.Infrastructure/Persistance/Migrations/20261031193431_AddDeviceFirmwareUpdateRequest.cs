using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceFirmwareUpdateRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProtectedCommandKey",
                schema: "Data",
                table: "MqttCredential",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CommandVersion",
                schema: "Data",
                table: "DeviceFirmwareUpdate",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                schema: "Data",
                table: "DeviceFirmwareUpdate",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql(
                """
                WITH ranked AS (
                    SELECT "Id",
                           ROW_NUMBER() OVER (PARTITION BY "MicroCtlDeviceId" ORDER BY "RequestedAt", "Id") AS command_version
                    FROM "Data"."DeviceFirmwareUpdate"
                )
                UPDATE "Data"."DeviceFirmwareUpdate" AS target
                SET "CommandVersion" = ranked.command_version,
                    "ExpiresAt" = target."RequestedAt" + INTERVAL '5 minutes'
                FROM ranked
                WHERE target."Id" = ranked."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFirmwareUpdate_MicroCtlDeviceId_CommandVersion",
                schema: "Data",
                table: "DeviceFirmwareUpdate",
                columns: new[] { "MicroCtlDeviceId", "CommandVersion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceFirmwareUpdate_MicroCtlDeviceId_CommandVersion",
                schema: "Data",
                table: "DeviceFirmwareUpdate");

            migrationBuilder.DropColumn(
                name: "ProtectedCommandKey",
                schema: "Data",
                table: "MqttCredential");

            migrationBuilder.DropColumn(
                name: "CommandVersion",
                schema: "Data",
                table: "DeviceFirmwareUpdate");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                schema: "Data",
                table: "DeviceFirmwareUpdate");
        }
    }
}
