using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChilliCoreTemplate.Data.Migrations
{
    public partial class migration_sms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SmsQueue",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: true),
                    TemplateId = table.Column<string>(maxLength: 50, nullable: true),
                    TemplateIdHash = table.Column<int>(nullable: false),
                    Data = table.Column<string>(maxLength: 1000, nullable: true),
                    MessageId = table.Column<string>(maxLength: 50, nullable: true),
                    MessageIdHash = table.Column<int>(nullable: false),
                    QueuedOn = table.Column<DateTime>(nullable: false),
                    IsReady = table.Column<bool>(nullable: false),
                    SentOn = table.Column<DateTime>(nullable: true),
                    DeliveredOn = table.Column<DateTime>(nullable: true),
                    ClickedOn = table.Column<DateTime>(nullable: true),
                    Error = table.Column<string>(nullable: true),
                    RetryCount = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsQueue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsQueue_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SmsQueue_MessageIdHash",
                table: "SmsQueue",
                column: "MessageIdHash");

            migrationBuilder.CreateIndex(
                name: "IX_SmsQueue_QueuedOn",
                table: "SmsQueue",
                column: "QueuedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SmsQueue_TemplateIdHash",
                table: "SmsQueue",
                column: "TemplateIdHash");

            migrationBuilder.CreateIndex(
                name: "IX_SmsQueue_UserId",
                table: "SmsQueue",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmsQueue");
        }
    }
}
