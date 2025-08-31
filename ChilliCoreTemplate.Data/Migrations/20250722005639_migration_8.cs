using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChilliCoreTemplate.Data.Migrations
{
    /// <inheritdoc />
    public partial class migration_8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(51)",
                maxLength: 51,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(55)",
                oldMaxLength: 55,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FirstNameIndex",
                table: "Users",
                type: "decimal(30,0)",
                precision: 30,
                scale: 0,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FullNameIndex",
                table: "Users",
                type: "decimal(30,0)",
                precision: 30,
                scale: 0,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "LastNameIndex",
                table: "Users",
                type: "decimal(30,0)",
                precision: 30,
                scale: 0,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Recipient",
                table: "Emails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_FirstNameIndex",
                table: "Users",
                column: "FirstNameIndex");

            migrationBuilder.CreateIndex(
                name: "IX_Users_FullNameIndex",
                table: "Users",
                column: "FullNameIndex");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastNameIndex",
                table: "Users",
                column: "LastNameIndex");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Status",
                table: "Users",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_Role_CompanyId",
                table: "UserRoles",
                columns: new[] { "UserId", "Role", "CompanyId" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserRoles_RoleStatus",
                table: "UserRoles",
                sql: "[Role] <> 2 or [Status] <> 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_FirstNameIndex",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_FullNameIndex",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastNameIndex",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Status",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_Role_CompanyId",
                table: "UserRoles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserRoles_RoleStatus",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "FirstNameIndex",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FullNameIndex",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastNameIndex",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(55)",
                maxLength: 55,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(51)",
                oldMaxLength: 51,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Recipient",
                table: "Emails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");
        }
    }
}
