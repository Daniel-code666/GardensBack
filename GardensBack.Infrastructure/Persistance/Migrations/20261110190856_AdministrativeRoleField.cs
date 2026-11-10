using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardensBack.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AdministrativeRoleField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdministrativeRole",
                schema: "Data",
                table: "Role",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdministrativeRole",
                schema: "Data",
                table: "Role");
        }
    }
}
