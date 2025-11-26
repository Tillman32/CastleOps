using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastleOps.Api.Migrations
{
    /// <inheritdoc />
    public partial class RedesignPeonDeviceRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketplaceItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PeonConfigs",
                table: "PeonConfigs");

            migrationBuilder.DropIndex(
                name: "IX_PeonConfigs_PeonId",
                table: "PeonConfigs");

            migrationBuilder.DropColumn(
                name: "Entry",
                table: "Peons");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Peons");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PeonConfigs");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "PeonConfigs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PeonConfigs",
                table: "PeonConfigs",
                columns: new[] { "PeonId", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Peons_Slug",
                table: "Peons",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Peons_Slug",
                table: "Peons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PeonConfigs",
                table: "PeonConfigs");

            migrationBuilder.AddColumn<string>(
                name: "Entry",
                table: "Peons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Peons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "PeonConfigs",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "PeonConfigs",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PeonConfigs",
                table: "PeonConfigs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MarketplaceItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    GitUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceItem", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeonConfigs_PeonId",
                table: "PeonConfigs",
                column: "PeonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceItem_GitUrl",
                table: "MarketplaceItem",
                column: "GitUrl",
                unique: true);
        }
    }
}
