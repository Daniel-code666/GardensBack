using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceConnectivityConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceConnectivityConfiguration",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfigurationKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ApiHost = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                    ApiPort = table.Column<int>(type: "integer", nullable: false),
                    ApiUseTls = table.Column<bool>(type: "boolean", nullable: false),
                    MqttHost = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                    MqttPort = table.Column<int>(type: "integer", nullable: false),
                    MqttUseTls = table.Column<bool>(type: "boolean", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceConnectivityConfiguration", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceConnectivityConfiguration_ConfigurationKey",
                schema: "Data",
                table: "DeviceConnectivityConfiguration",
                column: "ConfigurationKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceConnectivityConfiguration",
                schema: "Data");
        }
    }
}
