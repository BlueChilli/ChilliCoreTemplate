using Microsoft.EntityFrameworkCore.Migrations;

namespace ChilliCoreTemplate.Data.Migrations
{
    public partial class migration_indexes2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_PushToken",
                table: "UserDevices",
                column: "PushToken",
                unique: true,
                filter: "[PushToken] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDevices_PushToken",
                table: "UserDevices");
        }
    }
}
