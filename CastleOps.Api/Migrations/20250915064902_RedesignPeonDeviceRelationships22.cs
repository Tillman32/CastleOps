using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastleOps.Api.Migrations
{
    /// <inheritdoc />
    public partial class RedesignPeonDeviceRelationships22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultEntry",
                table: "Peons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultEnvironment",
                table: "Peons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultVersion",
                table: "Peons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultEntry",
                table: "Peons");

            migrationBuilder.DropColumn(
                name: "DefaultEnvironment",
                table: "Peons");

            migrationBuilder.DropColumn(
                name: "DefaultVersion",
                table: "Peons");
        }
    }
}
