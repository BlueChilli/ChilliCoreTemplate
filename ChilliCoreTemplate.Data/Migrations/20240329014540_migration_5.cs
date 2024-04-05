using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChilliCoreTemplate.Data.Migrations
{
    /// <inheritdoc />
    public partial class migration_5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "BulkImports");

            migrationBuilder.AddColumn<bool>(
                name: "StripeCompleted",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FilesJson",
                table: "BulkImports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parameters",
                table: "BulkImports",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeCompleted",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "FilesJson",
                table: "BulkImports");

            migrationBuilder.DropColumn(
                name: "Parameters",
                table: "BulkImports");

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "BulkImports",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
