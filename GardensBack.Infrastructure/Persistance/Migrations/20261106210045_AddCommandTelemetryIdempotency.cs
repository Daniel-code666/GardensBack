using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddCommandTelemetryIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "Data"."DeviceTelemetry" AS duplicate
                USING "Data"."DeviceTelemetry" AS original
                WHERE duplicate."RequestId" IS NOT NULL
                  AND duplicate."DeviceId" = original."DeviceId"
                  AND duplicate."RequestId" = original."RequestId"
                  AND duplicate."Id" > original."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTelemetry_DeviceId_RequestId",
                schema: "Data",
                table: "DeviceTelemetry",
                columns: new[] { "DeviceId", "RequestId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceTelemetry_DeviceId_RequestId",
                schema: "Data",
                table: "DeviceTelemetry");
        }
    }
}
