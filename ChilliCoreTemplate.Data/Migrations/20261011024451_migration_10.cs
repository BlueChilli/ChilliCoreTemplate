using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChilliCoreTemplate.Data.Migrations
{
    /// <inheritdoc />
    public partial class migration_10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Webhooks_Inbound");

            migrationBuilder.AddColumn<int>(
                name: "UserRoleId",
                table: "UserTokens",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ByUserId",
                table: "UserActivities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QueuedOn",
                table: "BulkImports",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "ResponseSize",
                table: "ApiLogEntries",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WebhooksInbound",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebhookId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WebhookIdHash = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Subtype = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubtypeId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Raw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhooksInbound", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserRoleId",
                table: "UserTokens",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_ByUserId",
                table: "UserActivities",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhooksInbound_Processed",
                table: "WebhooksInbound",
                column: "Processed");

            migrationBuilder.CreateIndex(
                name: "IX_WebhooksInbound_WebhookIdHash",
                table: "WebhooksInbound",
                column: "WebhookIdHash");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivities_Users_ByUserId",
                table: "UserActivities",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_UserRoles_UserRoleId",
                table: "UserTokens",
                column: "UserRoleId",
                principalTable: "UserRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivities_Users_ByUserId",
                table: "UserActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_UserRoles_UserRoleId",
                table: "UserTokens");

            migrationBuilder.DropTable(
                name: "WebhooksInbound");

            migrationBuilder.DropIndex(
                name: "IX_UserTokens_UserRoleId",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserActivities_ByUserId",
                table: "UserActivities");

            migrationBuilder.DropColumn(
                name: "UserRoleId",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "ByUserId",
                table: "UserActivities");

            migrationBuilder.DropColumn(
                name: "QueuedOn",
                table: "BulkImports");

            migrationBuilder.DropColumn(
                name: "ResponseSize",
                table: "ApiLogEntries");

            migrationBuilder.CreateTable(
                name: "Webhooks_Inbound",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Processed = table.Column<bool>(type: "bit", nullable: false),
                    Raw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subtype = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    WebhookId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WebhookIdHash = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webhooks_Inbound", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Webhooks_Inbound_WebhookIdHash",
                table: "Webhooks_Inbound",
                column: "WebhookIdHash");
        }
    }
}
