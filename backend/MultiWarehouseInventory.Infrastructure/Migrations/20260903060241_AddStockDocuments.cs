using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiWarehouseInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StockDocumentId",
                table: "StockMovements",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "StockDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DocumentCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WarehouseId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PartnerName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TenantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockDocuments_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StockDocumentLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StockDocumentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProductId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WarehouseZoneId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockDocumentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockDocumentLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockDocumentLines_StockDocuments_StockDocumentId",
                        column: x => x.StockDocumentId,
                        principalTable: "StockDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockDocumentLines_WarehouseZones_WarehouseZoneId",
                        column: x => x.WarehouseZoneId,
                        principalTable: "WarehouseZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_StockDocumentId",
                table: "StockMovements",
                column: "StockDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocumentLines_ProductId",
                table: "StockDocumentLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocumentLines_StockDocumentId_ProductId_WarehouseZoneId",
                table: "StockDocumentLines",
                columns: new[] { "StockDocumentId", "ProductId", "WarehouseZoneId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockDocumentLines_WarehouseZoneId",
                table: "StockDocumentLines",
                column: "WarehouseZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_TenantId_DocumentCode",
                table: "StockDocuments",
                columns: new[] { "TenantId", "DocumentCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_TenantId_WarehouseId_DocumentDate",
                table: "StockDocuments",
                columns: new[] { "TenantId", "WarehouseId", "DocumentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_WarehouseId",
                table: "StockDocuments",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockDocuments_StockDocumentId",
                table: "StockMovements",
                column: "StockDocumentId",
                principalTable: "StockDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockDocuments_StockDocumentId",
                table: "StockMovements");

            migrationBuilder.DropTable(
                name: "StockDocumentLines");

            migrationBuilder.DropTable(
                name: "StockDocuments");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_StockDocumentId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "StockDocumentId",
                table: "StockMovements");
        }
    }
}
