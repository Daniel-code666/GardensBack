using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class FirmwareArtifact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Available",
                schema: "Data",
                table: "Device",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                schema: "Data",
                table: "Device",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FirmwareArtifact",
                schema: "Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    FileName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirmwareReleaseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmwareArtifact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirmwareArtifact_FirmwareRelease_FirmwareReleaseId",
                        column: x => x.FirmwareReleaseId,
                        principalSchema: "Data",
                        principalTable: "FirmwareRelease",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FirmwareArtifact_FirmwareReleaseId",
                schema: "Data",
                table: "FirmwareArtifact",
                column: "FirmwareReleaseId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FirmwareArtifact",
                schema: "Data");

            migrationBuilder.DropColumn(
                name: "Available",
                schema: "Data",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                schema: "Data",
                table: "Device");
        }
    }
}
