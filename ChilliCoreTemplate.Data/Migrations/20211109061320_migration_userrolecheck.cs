using Microsoft.EntityFrameworkCore.Migrations;

namespace ChilliCoreTemplate.Data.Migrations
{
    public partial class migration_userrolecheck : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDevices_PushToken",
                table: "UserDevices");

            migrationBuilder.DropColumn(
                name: "MobileToken",
                table: "UserDevices");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserRoles_Role",
                table: "UserRoles",
                sql: "[Role] > 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserRoles_Role",
                table: "UserRoles");

            migrationBuilder.AddColumn<string>(
                name: "MobileToken",
                table: "UserDevices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_PushToken",
                table: "UserDevices",
                column: "PushToken",
                unique: true,
                filter: "[PushToken] IS NOT NULL");
        }
    }
}
