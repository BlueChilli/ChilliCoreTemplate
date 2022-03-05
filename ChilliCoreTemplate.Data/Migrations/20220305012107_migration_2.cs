using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChilliCoreTemplate.Data.Migrations
{
    public partial class migration_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MasterCompanyId",
                table: "Companies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_MasterCompanyId",
                table: "Companies",
                column: "MasterCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Companies_MasterCompanyId",
                table: "Companies",
                column: "MasterCompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Companies_MasterCompanyId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_MasterCompanyId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "MasterCompanyId",
                table: "Companies");
        }
    }
}
