using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastleOps.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPeonConfigRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Peons_MarketplaceId",
                table: "Peons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MarketplaceItems",
                table: "MarketplaceItems");

            migrationBuilder.DropColumn(
                name: "ScriptType",
                table: "Peons");

            migrationBuilder.RenameTable(
                name: "MarketplaceItems",
                newName: "MarketplaceItem");

            migrationBuilder.RenameColumn(
                name: "MarketplaceId",
                table: "Peons",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "GitUrl",
                table: "Peons",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "EntryPoint",
                table: "Peons",
                newName: "Slug");

            migrationBuilder.RenameIndex(
                name: "IX_MarketplaceItems_GitUrl",
                table: "MarketplaceItem",
                newName: "IX_MarketplaceItem_GitUrl");

            migrationBuilder.AddColumn<string>(
                name: "Entry",
                table: "Peons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MarketplaceItem",
                table: "MarketplaceItem",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PeonConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false),
                    Entry = table.Column<string>(type: "TEXT", nullable: false),
                    Environment = table.Column<string>(type: "TEXT", nullable: false),
                    PeonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeonConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeonConfigs_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeonConfigs_Peons_PeonId",
                        column: x => x.PeonId,
                        principalTable: "Peons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeonConfigs_DeviceId",
                table: "PeonConfigs",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_PeonConfigs_PeonId",
                table: "PeonConfigs",
                column: "PeonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeonConfigs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MarketplaceItem",
                table: "MarketplaceItem");

            migrationBuilder.DropColumn(
                name: "Entry",
                table: "Peons");

            migrationBuilder.RenameTable(
                name: "MarketplaceItem",
                newName: "MarketplaceItems");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Peons",
                newName: "MarketplaceId");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Peons",
                newName: "GitUrl");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "Peons",
                newName: "EntryPoint");

            migrationBuilder.RenameIndex(
                name: "IX_MarketplaceItem_GitUrl",
                table: "MarketplaceItems",
                newName: "IX_MarketplaceItems_GitUrl");

            migrationBuilder.AddColumn<int>(
                name: "ScriptType",
                table: "Peons",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MarketplaceItems",
                table: "MarketplaceItems",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Peons_MarketplaceId",
                table: "Peons",
                column: "MarketplaceId",
                unique: true);
        }
    }
}
