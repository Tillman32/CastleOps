using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastleOps.Api.Migrations
{
    /// <inheritdoc />
    public partial class RedesignPeonDeviceRelationships222 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DefaultEntry",
                table: "Peons",
                newName: "Entry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Entry",
                table: "Peons",
                newName: "DefaultEntry");
        }
    }
}
