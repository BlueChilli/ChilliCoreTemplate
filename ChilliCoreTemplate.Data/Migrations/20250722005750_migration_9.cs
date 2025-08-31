using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChilliCoreTemplate.Data.Migrations
{
    /// <inheritdoc />
    public partial class migration_9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Guid",
                table: "Users",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Guid",
                table: "Users");
        }
    }
}
