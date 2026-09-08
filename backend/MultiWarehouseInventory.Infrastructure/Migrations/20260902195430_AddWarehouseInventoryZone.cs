using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiWarehouseInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseInventoryZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The existing composite index is also the supporting index for the
            // Warehouse FK. Keep that FK valid while replacing the unique key.
            migrationBuilder.CreateIndex(
                name: "IX_WarehouseInventories_WarehouseId",
                table: "WarehouseInventories",
                column: "WarehouseId");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseInventories_WarehouseId_ProductId",
                table: "WarehouseInventories");

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseZoneId",
                table: "WarehouseInventories",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseInventories_WarehouseId_ProductId_WarehouseZoneId",
                table: "WarehouseInventories",
                columns: new[] { "WarehouseId", "ProductId", "WarehouseZoneId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseInventories_WarehouseZoneId",
                table: "WarehouseInventories",
                column: "WarehouseZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseInventories_WarehouseZones_WarehouseZoneId",
                table: "WarehouseInventories",
                column: "WarehouseZoneId",
                principalTable: "WarehouseZones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropIndex(
                name: "IX_WarehouseInventories_WarehouseId",
                table: "WarehouseInventories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseInventories_WarehouseZones_WarehouseZoneId",
                table: "WarehouseInventories");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseInventories_WarehouseZoneId",
                table: "WarehouseInventories");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseInventories_WarehouseId",
                table: "WarehouseInventories",
                column: "WarehouseId");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseInventories_WarehouseId_ProductId_WarehouseZoneId",
                table: "WarehouseInventories");

            migrationBuilder.DropColumn(
                name: "WarehouseZoneId",
                table: "WarehouseInventories");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseInventories_WarehouseId_ProductId",
                table: "WarehouseInventories",
                columns: new[] { "WarehouseId", "ProductId" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_WarehouseInventories_WarehouseId",
                table: "WarehouseInventories");
        }
    }
}
