using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Data");

            migrationBuilder.CreateTable(
                name: "FirmwareRelease",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReleaseId = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    Board = table.Column<string>(type: "text", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "text", nullable: false),
                    FirmwareUrl = table.Column<string>(type: "text", nullable: false),
                    Signature = table.Column<string>(type: "text", nullable: false),
                    ContractVersion = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmwareRelease", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Data",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MicrocontrollerDevice",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    HardwareId = table.Column<string>(type: "text", nullable: true),
                    MqttClientId = table.Column<string>(type: "text", nullable: true),
                    Board = table.Column<string>(type: "text", nullable: true),
                    FirmwareVersion = table.Column<string>(type: "text", nullable: true),
                    ProvisioningState = table.Column<int>(type: "integer", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UnclaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRecoveryCommandVersion = table.Column<int>(type: "integer", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: true),
                    DeviceIp = table.Column<string>(type: "text", nullable: true),
                    DeviceSSID = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MicrocontrollerDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MicrocontrollerDevice_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Data",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Device",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocalDeviceId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DevicePin = table.Column<int>(type: "integer", nullable: true),
                    ConnectionType = table.Column<int>(type: "integer", nullable: false),
                    MeasurementType = table.Column<int>(type: "integer", nullable: true),
                    MeasurementUnit = table.Column<int>(type: "integer", nullable: true),
                    DeviceType = table.Column<int>(type: "integer", nullable: false),
                    MicroCtrlDeviceId = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Device_MicrocontrollerDevice_MicroCtrlDeviceId",
                        column: x => x.MicroCtrlDeviceId,
                        principalSchema: "Data",
                        principalTable: "MicrocontrollerDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceClaim",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<string>(type: "text", nullable: true),
                    ClaimCodeHash = table.Column<string>(type: "text", nullable: true),
                    ContractVersion = table.Column<int>(type: "integer", nullable: false),
                    DeviceClaimStatus = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorCode = table.Column<string>(type: "text", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    MicrocontrollerDeviceId = table.Column<int>(type: "integer", nullable: false),
                    ResolvedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceClaim_MicrocontrollerDevice_MicrocontrollerDeviceId",
                        column: x => x.MicrocontrollerDeviceId,
                        principalSchema: "Data",
                        principalTable: "MicrocontrollerDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceClaim_User_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalSchema: "Data",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceCommand",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Command = table.Column<string>(type: "text", nullable: false),
                    RequestId = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: true),
                    ResponsePayload = table.Column<string>(type: "text", nullable: true),
                    ErrorCode = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MicroCtrlDeviceId = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCommand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceCommand_MicrocontrollerDevice_MicroCtrlDeviceId",
                        column: x => x.MicroCtrlDeviceId,
                        principalSchema: "Data",
                        principalTable: "MicrocontrollerDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceFirmwareUpdate",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PreviousVersion = table.Column<string>(type: "text", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PendingValidationAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProgressPercentage = table.Column<int>(type: "integer", nullable: true),
                    FailureCode = table.Column<string>(type: "text", nullable: true),
                    FailureMessage = table.Column<string>(type: "text", nullable: true),
                    MicroCtlDeviceId = table.Column<int>(type: "integer", nullable: false),
                    FirmwareReleaseId = table.Column<int>(type: "integer", nullable: false),
                    RequestedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceFirmwareUpdate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceFirmwareUpdate_FirmwareRelease_FirmwareReleaseId",
                        column: x => x.FirmwareReleaseId,
                        principalSchema: "Data",
                        principalTable: "FirmwareRelease",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceFirmwareUpdate_MicrocontrollerDevice_MicroCtlDeviceId",
                        column: x => x.MicroCtlDeviceId,
                        principalSchema: "Data",
                        principalTable: "MicrocontrollerDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceFirmwareUpdate_User_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalSchema: "Data",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MqttCredential",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    RequestId = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    MqttClientId = table.Column<string>(type: "text", nullable: true),
                    Host = table.Column<string>(type: "text", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    SecretReference = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureCode = table.Column<string>(type: "text", nullable: true),
                    MicrocontrollerDeviceId = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MqttCredential", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MqttCredential_MicrocontrollerDevice_MicrocontrollerDeviceId",
                        column: x => x.MicrocontrollerDeviceId,
                        principalSchema: "Data",
                        principalTable: "MicrocontrollerDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTelemetry",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<float>(type: "real", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    RequestId = table.Column<string>(type: "text", nullable: true),
                    ActuatorMode = table.Column<int>(type: "integer", nullable: true),
                    State = table.Column<bool>(type: "boolean", nullable: true),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTelemetry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceTelemetry_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "Data",
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Device_MicroCtrlDeviceId",
                schema: "Data",
                table: "Device",
                column: "MicroCtrlDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceClaim_DeviceClaimStatus_ExpiresAt",
                schema: "Data",
                table: "DeviceClaim",
                columns: new[] { "DeviceClaimStatus", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceClaim_MicrocontrollerDeviceId",
                schema: "Data",
                table: "DeviceClaim",
                column: "MicrocontrollerDeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceClaim_RequestId",
                schema: "Data",
                table: "DeviceClaim",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceClaim_ResolvedByUserId",
                schema: "Data",
                table: "DeviceClaim",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommand_MicroCtrlDeviceId_RequestId",
                schema: "Data",
                table: "DeviceCommand",
                columns: new[] { "MicroCtrlDeviceId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommand_Status_ExpiresAt",
                schema: "Data",
                table: "DeviceCommand",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFirmwareUpdate_FirmwareReleaseId",
                schema: "Data",
                table: "DeviceFirmwareUpdate",
                column: "FirmwareReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFirmwareUpdate_MicroCtlDeviceId_CreationDate",
                schema: "Data",
                table: "DeviceFirmwareUpdate",
                columns: new[] { "MicroCtlDeviceId", "CreationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFirmwareUpdate_MicroCtlDeviceId_RequestId",
                schema: "Data",
                table: "DeviceFirmwareUpdate",
                columns: new[] { "MicroCtlDeviceId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFirmwareUpdate_RequestedByUserId",
                schema: "Data",
                table: "DeviceFirmwareUpdate",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFirmwareUpdate_Status_CreationDate",
                schema: "Data",
                table: "DeviceFirmwareUpdate",
                columns: new[] { "Status", "CreationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTelemetry_DeviceId_CreationDate",
                schema: "Data",
                table: "DeviceTelemetry",
                columns: new[] { "DeviceId", "CreationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FirmwareRelease_Board_Version",
                schema: "Data",
                table: "FirmwareRelease",
                columns: new[] { "Board", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FirmwareRelease_ReleaseId",
                schema: "Data",
                table: "FirmwareRelease",
                column: "ReleaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MicrocontrollerDevice_DeviceId",
                schema: "Data",
                table: "MicrocontrollerDevice",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MicrocontrollerDevice_HardwareId",
                schema: "Data",
                table: "MicrocontrollerDevice",
                column: "HardwareId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MicrocontrollerDevice_MqttClientId",
                schema: "Data",
                table: "MicrocontrollerDevice",
                column: "MqttClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MicrocontrollerDevice_PublicId",
                schema: "Data",
                table: "MicrocontrollerDevice",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MicrocontrollerDevice_UserId",
                schema: "Data",
                table: "MicrocontrollerDevice",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MqttCredential_MicrocontrollerDeviceId",
                schema: "Data",
                table: "MqttCredential",
                column: "MicrocontrollerDeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MqttCredential_RequestId",
                schema: "Data",
                table: "MqttCredential",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MqttCredential_Version",
                schema: "Data",
                table: "MqttCredential",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                schema: "Data",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                schema: "Data",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                schema: "Data",
                table: "User",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceClaim",
                schema: "Data");

            migrationBuilder.DropTable(
                name: "DeviceCommand",
                schema: "Data");

            migrationBuilder.DropTable(
                name: "DeviceFirmwareUpdate",
                schema: "Data");

            migrationBuilder.DropTable(
                name: "DeviceTelemetry",
                schema: "Data");

            migrationBuilder.DropTable(
                name: "MqttCredential",
                schema: "Data");

            migrationBuilder.DropTable(
                name: "FirmwareRelease",
                schema: "Data");

            migrationBuilder.DropTable(
                name: "Device",
                schema: "Data");

            migrationBuilder.DropTable(
                name: "MicrocontrollerDevice",
                schema: "Data");

            migrationBuilder.DropTable(
                name: "User",
                schema: "Data");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "Data");
        }
    }
}
