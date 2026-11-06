using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceCommandVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CommandVersion",
                schema: "Data",
                table: "DeviceCommand",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql(
                """
                WITH ranked AS (
                    SELECT "Id",
                           ROW_NUMBER() OVER (
                               PARTITION BY "MicroCtrlDeviceId"
                               ORDER BY "CreationDate", "Id") AS command_version
                    FROM "Data"."DeviceCommand"
                )
                UPDATE "Data"."DeviceCommand" AS command
                SET "CommandVersion" = ranked.command_version
                FROM ranked
                WHERE command."Id" = ranked."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommand_MicroCtrlDeviceId_CommandVersion",
                schema: "Data",
                table: "DeviceCommand",
                columns: new[] { "MicroCtrlDeviceId", "CommandVersion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceCommand_MicroCtrlDeviceId_CommandVersion",
                schema: "Data",
                table: "DeviceCommand");

            migrationBuilder.DropColumn(
                name: "CommandVersion",
                schema: "Data",
                table: "DeviceCommand");
        }
    }
}
