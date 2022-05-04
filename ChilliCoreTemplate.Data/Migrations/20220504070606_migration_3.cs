using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChilliCoreTemplate.Data.Migrations
{
    public partial class migration_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BulkImports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Records = table.Column<int>(type: "int", nullable: false),
                    Processed = table.Column<int>(type: "int", nullable: false),
                    Errors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulkImports_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChargeId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaidOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReceiptUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BulkImports_CompanyId",
                table: "BulkImports",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CompanyId",
                table: "Payments",
                column: "CompanyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BulkImports");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
