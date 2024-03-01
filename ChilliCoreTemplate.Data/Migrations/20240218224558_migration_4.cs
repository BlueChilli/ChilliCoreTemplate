using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChilliCoreTemplate.Data.Migrations
{
    /// <inheritdoc />
    public partial class migration_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMfaVerified",
                table: "UserSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMfaEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "UserDevices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserDeviceId",
                table: "PushNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TemplateId",
                table: "Emails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMasterCompany",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_UserDeviceId",
                table: "PushNotifications",
                column: "UserDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_TrackingId",
                table: "Emails",
                column: "TrackingId");

            migrationBuilder.AddForeignKey(
                name: "FK_PushNotifications_UserDevices_UserDeviceId",
                table: "PushNotifications",
                column: "UserDeviceId",
                principalTable: "UserDevices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PushNotifications_UserDevices_UserDeviceId",
                table: "PushNotifications");

            migrationBuilder.DropIndex(
                name: "IX_PushNotifications_UserDeviceId",
                table: "PushNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Emails_TrackingId",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "IsMfaVerified",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "IsMfaEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "UserDevices");

            migrationBuilder.DropColumn(
                name: "UserDeviceId",
                table: "PushNotifications");

            migrationBuilder.DropColumn(
                name: "IsMasterCompany",
                table: "Companies");

            migrationBuilder.AlterColumn<string>(
                name: "TemplateId",
                table: "Emails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
