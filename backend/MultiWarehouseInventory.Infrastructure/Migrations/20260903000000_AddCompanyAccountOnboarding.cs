using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MultiWarehouseInventory.Infrastructure;

#nullable disable

namespace MultiWarehouseInventory.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260903000000_AddCompanyAccountOnboarding")]
public partial class AddCompanyAccountOnboarding : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "TenantId",
            table: "Users",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

        migrationBuilder.AddColumn<DateTime>(
            name: "EmailVerificationTokenExpiresAt",
            table: "Users",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "EmailVerificationTokenHash",
            table: "Users",
            type: "longtext",
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<DateTime>(
            name: "EmailVerifiedAt",
            table: "Users",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "MustChangePassword",
            table: "Users",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Users",
            type: "varchar(254)",
            maxLength: 254,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "longtext");

        migrationBuilder.AlterColumn<string>(
            name: "Username",
            table: "Users",
            type: "varchar(254)",
            maxLength: 254,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "longtext");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_TenantId",
            table: "Users",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Username",
            table: "Users",
            column: "Username",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Tenants_TenantId",
            table: "Users",
            column: "TenantId",
            principalTable: "Tenants",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        // Keep existing accounts usable, while moving every prior company account
        // into the new mandatory password-change policy.
        migrationBuilder.Sql("UPDATE `Users` SET `EmailVerifiedAt` = UTC_TIMESTAMP(6), `MustChangePassword` = 1 WHERE `TenantId` IS NOT NULL;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Users_Tenants_TenantId",
            table: "Users");

        migrationBuilder.DropIndex(name: "IX_Users_Email", table: "Users");
        migrationBuilder.DropIndex(name: "IX_Users_TenantId", table: "Users");
        migrationBuilder.DropIndex(name: "IX_Users_Username", table: "Users");

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Users",
            type: "longtext",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(254)",
            oldMaxLength: 254);

        migrationBuilder.AlterColumn<string>(
            name: "Username",
            table: "Users",
            type: "longtext",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(254)",
            oldMaxLength: 254);

        migrationBuilder.DropColumn(name: "EmailVerificationTokenExpiresAt", table: "Users");
        migrationBuilder.DropColumn(name: "EmailVerificationTokenHash", table: "Users");
        migrationBuilder.DropColumn(name: "EmailVerifiedAt", table: "Users");
        migrationBuilder.DropColumn(name: "MustChangePassword", table: "Users");
        migrationBuilder.DropColumn(name: "TenantId", table: "Users");
    }
}
