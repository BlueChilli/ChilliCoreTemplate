using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChilliCoreTemplate.Data.Migrations
{
    /// <inheritdoc />
    public partial class migration_7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDelivered",
                table: "Emails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MessageId",
                table: "Emails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MessageIdHash",
                table: "Emails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Emails_MessageIdHash",
                table: "Emails",
                column: "MessageIdHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emails_MessageIdHash",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "IsDelivered",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "MessageIdHash",
                table: "Emails");
        }
    }
}
