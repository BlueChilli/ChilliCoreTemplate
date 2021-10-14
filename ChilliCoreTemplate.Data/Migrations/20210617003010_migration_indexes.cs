using Microsoft.EntityFrameworkCore.Migrations;

namespace ChilliCoreTemplate.Data.Migrations
{
    public partial class migration_indexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Webhooks_Inbound_WebhookIdHash",
                table: "Webhooks_Inbound",
                column: "WebhookIdHash");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_TimeStamp",
                table: "ErrorLogs",
                column: "TimeStamp");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_StripeId",
                table: "Companies",
                column: "StripeId",
                unique: true,
                filter: "[StripeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogEntries_ResponseStatusCode",
                table: "ApiLogEntries",
                column: "ResponseStatusCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Webhooks_Inbound_WebhookIdHash",
                table: "Webhooks_Inbound");

            migrationBuilder.DropIndex(
                name: "IX_ErrorLogs_TimeStamp",
                table: "ErrorLogs");

            migrationBuilder.DropIndex(
                name: "IX_Companies_StripeId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_ApiLogEntries_ResponseStatusCode",
                table: "ApiLogEntries");
        }
    }
}
