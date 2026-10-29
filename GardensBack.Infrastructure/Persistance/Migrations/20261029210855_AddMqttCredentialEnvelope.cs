using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddMqttCredentialEnvelope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponseAuthenticationTag",
                schema: "Data",
                table: "MqttCredential",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseCiphertext",
                schema: "Data",
                table: "MqttCredential",
                type: "character varying(4096)",
                maxLength: 4096,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseNonce",
                schema: "Data",
                table: "MqttCredential",
                type: "character varying(24)",
                maxLength: 24,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseAuthenticationTag",
                schema: "Data",
                table: "MqttCredential");

            migrationBuilder.DropColumn(
                name: "ResponseCiphertext",
                schema: "Data",
                table: "MqttCredential");

            migrationBuilder.DropColumn(
                name: "ResponseNonce",
                schema: "Data",
                table: "MqttCredential");
        }
    }
}
