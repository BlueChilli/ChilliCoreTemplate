using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChilliCoreTemplate.Data.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiLogEntries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User = table.Column<string>(maxLength: 100, nullable: true),
                    Machine = table.Column<string>(maxLength: 100, nullable: true),
                    RequestIpAddress = table.Column<string>(maxLength: 100, nullable: true),
                    RequestContentType = table.Column<string>(maxLength: 100, nullable: true),
                    RequestContentBody = table.Column<string>(nullable: true),
                    RequestUri = table.Column<string>(nullable: true),
                    RequestMethod = table.Column<string>(maxLength: 100, nullable: true),
                    RequestHeaders = table.Column<string>(nullable: true),
                    RequestTimestamp = table.Column<DateTime>(nullable: false),
                    ResponseContentType = table.Column<string>(maxLength: 100, nullable: true),
                    ResponseContentBody = table.Column<string>(nullable: true),
                    ResponseStatusCode = table.Column<int>(nullable: true),
                    ResponseHeaders = table.Column<string>(nullable: true),
                    ResponseTimestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiLogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistributedLocks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Resource = table.Column<Guid>(nullable: false),
                    LockReference = table.Column<int>(nullable: false),
                    Timeout = table.Column<long>(nullable: false),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedByMachine = table.Column<string>(maxLength: 100, nullable: true),
                    LockedByPID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributedLocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecurrentTasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Identifier = table.Column<Guid>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    Interval = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurrentTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneVerificationCode = table.Column<string>(maxLength: 100, nullable: true),
                    PhoneVerificationToken = table.Column<Guid>(nullable: true),
                    PhoneVerificationExpiry = table.Column<DateTime>(nullable: true),
                    PhoneVerificationRetries = table.Column<int>(nullable: false),
                    Email = table.Column<string>(maxLength: 100, nullable: true),
                    EmailHash = table.Column<int>(nullable: true),
                    FirstName = table.Column<string>(maxLength: 25, nullable: true),
                    LastName = table.Column<string>(maxLength: 25, nullable: true),
                    FullName = table.Column<string>(maxLength: 55, nullable: true),
                    Phone = table.Column<string>(maxLength: 20, nullable: true),
                    PhoneHash = table.Column<int>(nullable: false),
                    ProfilePhotoPath = table.Column<string>(maxLength: 100, nullable: true),
                    Status = table.Column<int>(nullable: false),
                    PasswordHash = table.Column<string>(maxLength: 256, nullable: true),
                    PasswordSalt = table.Column<Guid>(nullable: false),
                    PasswordAutoGenerated = table.Column<bool>(nullable: false),
                    LastLoginDate = table.Column<DateTime>(nullable: true),
                    LastRetryDate = table.Column<DateTime>(nullable: true),
                    NumOfRetries = table.Column<int>(nullable: false),
                    LoginCount = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    ActivatedDate = table.Column<DateTime>(nullable: true),
                    ClosedDate = table.Column<DateTime>(nullable: true),
                    LastPasswordChangedDate = table.Column<DateTime>(nullable: true),
                    InvitedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Webhooks_Inbound",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebhookId = table.Column<string>(maxLength: 100, nullable: true),
                    WebhookIdHash = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Subtype = table.Column<string>(maxLength: 100, nullable: true),
                    Raw = table.Column<string>(nullable: true),
                    Error = table.Column<string>(nullable: true),
                    Success = table.Column<bool>(nullable: false),
                    Processed = table.Column<bool>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webhooks_Inbound", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SingleTasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Identifier = table.Column<Guid>(nullable: false),
                    JsonParameters = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    StatusChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecurrentTaskId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SingleTasks_RecurrentTasks_RecurrentTaskId",
                        column: x => x.RecurrentTaskId,
                        principalTable: "RecurrentTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    ApiKey = table.Column<Guid>(nullable: false),
                    StripeId = table.Column<string>(maxLength: 50, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    LogoPath = table.Column<string>(maxLength: 100, nullable: true),
                    Website = table.Column<string>(maxLength: 100, nullable: true),
                    Notes = table.Column<string>(maxLength: 1000, nullable: true),
                    Timezone = table.Column<string>(maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeletedAt = table.Column<DateTime>(nullable: true),
                    DeletedById = table.Column<int>(nullable: true),
                    IsSetup = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Users_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    TemplateId = table.Column<string>(maxLength: 50, nullable: true),
                    TemplateIdHash = table.Column<int>(nullable: false),
                    Recipient = table.Column<string>(maxLength: 100, nullable: true),
                    Model = table.Column<string>(nullable: true),
                    Attachments = table.Column<string>(maxLength: 100, nullable: true),
                    DateQueued = table.Column<DateTime>(nullable: false),
                    IsReady = table.Column<bool>(nullable: false),
                    IsSent = table.Column<bool>(nullable: false),
                    IsSending = table.Column<bool>(nullable: false),
                    DateSent = table.Column<DateTime>(nullable: true),
                    IsOpened = table.Column<bool>(nullable: false),
                    OpenDate = table.Column<DateTime>(nullable: true),
                    OpenCount = table.Column<int>(nullable: false),
                    IsClicked = table.Column<bool>(nullable: false),
                    ClickDate = table.Column<DateTime>(nullable: true),
                    ClickCount = table.Column<int>(nullable: false),
                    IsUnsubscribed = table.Column<bool>(nullable: false),
                    UnsubscribeDate = table.Column<DateTime>(nullable: true),
                    Error = table.Column<string>(nullable: true),
                    RetryCount = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emails_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    TemplateId = table.Column<string>(nullable: true),
                    TemplateIdHash = table.Column<int>(nullable: false),
                    IsUnsubscribed = table.Column<bool>(nullable: false),
                    UnsubscribeDate = table.Column<DateTime>(nullable: true),
                    Reason = table.Column<int>(nullable: true),
                    ReasonOther = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    MessageTemplate = table.Column<string>(nullable: true),
                    Level = table.Column<string>(maxLength: 128, nullable: true),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    ExceptionMessage = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    LogEvent = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErrorLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PushNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Provider = table.Column<int>(nullable: false),
                    MessageId = table.Column<string>(maxLength: 100, nullable: true),
                    Message = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    OpenedOn = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Error = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    ActivityType = table.Column<int>(nullable: false),
                    EntityType = table.Column<int>(nullable: false),
                    EntityId = table.Column<int>(nullable: false),
                    TargetId = table.Column<int>(nullable: true),
                    ActivityOn = table.Column<DateTime>(nullable: false),
                    JsonData = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(maxLength: 100, nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    PinToken = table.Column<Guid>(nullable: false),
                    PinHash = table.Column<string>(maxLength: 200, nullable: true),
                    PinRetries = table.Column<int>(nullable: false),
                    PinLastRetryDate = table.Column<DateTime>(nullable: true),
                    PushToken = table.Column<string>(maxLength: 200, nullable: true),
                    PushTokenId = table.Column<string>(maxLength: 500, nullable: true),
                    PushProvider = table.Column<int>(nullable: true),
                    PushAppId = table.Column<int>(nullable: true),
                    MobileToken = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Token = table.Column<Guid>(nullable: false),
                    Expiry = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    Timezone = table.Column<string>(maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Role = table.Column<int>(nullable: false),
                    CompanyId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    UserDeviceId = table.Column<int>(nullable: true),
                    SessionId = table.Column<Guid>(nullable: false),
                    SessionCreatedOn = table.Column<DateTime>(nullable: false),
                    SessionExpiryOn = table.Column<DateTime>(nullable: false),
                    ImpersonationChain = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_UserDevices_UserDeviceId",
                        column: x => x.UserDeviceId,
                        principalTable: "UserDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationUsers_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogEntries_RequestTimestamp",
                table: "ApiLogEntries",
                column: "RequestTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_DeletedById",
                table: "Companies",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Company_Guid",
                table: "Companies",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DistributedLocks_Resource",
                table: "DistributedLocks",
                column: "Resource",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Emails_DateQueued",
                table: "Emails",
                column: "DateQueued");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_TemplateIdHash",
                table: "Emails",
                column: "TemplateIdHash");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_UserId",
                table: "Emails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailUsers_UserId",
                table: "EmailUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_UserId",
                table: "ErrorLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CompanyId",
                table: "Locations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationUsers_UserId",
                table: "LocationUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationUsers_LocationId_UserId",
                table: "LocationUsers",
                columns: new[] { "LocationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_CreatedOn",
                table: "PushNotifications",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_UserId",
                table: "PushNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurrentTasks_Identifier",
                table: "RecurrentTasks",
                column: "Identifier");

            migrationBuilder.CreateIndex(
                name: "IX_SingleTasks_Identifier",
                table: "SingleTasks",
                column: "Identifier");

            migrationBuilder.CreateIndex(
                name: "IX_CreateSingleTask",
                table: "SingleTasks",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_SingleTasks_RecurrentTaskId_Status",
                table: "SingleTasks",
                columns: new[] { "RecurrentTaskId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SingleTasks_Status_LockedUntil",
                table: "SingleTasks",
                columns: new[] { "Status", "LockedUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_UserId",
                table: "UserActivities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_PinToken",
                table: "UserDevices",
                column: "PinToken");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId",
                table: "UserDevices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_CompanyId",
                table: "UserRoles",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedDate",
                table: "Users",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailHash",
                table: "Users",
                column: "EmailHash");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneHash",
                table: "Users",
                column: "PhoneHash");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_SessionExpiryOn",
                table: "UserSessions",
                column: "SessionExpiryOn");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_SessionId",
                table: "UserSessions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserDeviceId",
                table: "UserSessions",
                column: "UserDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_Token",
                table: "UserTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId",
                table: "UserTokens",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiLogEntries");

            migrationBuilder.DropTable(
                name: "DistributedLocks");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "EmailUsers");

            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.DropTable(
                name: "LocationUsers");

            migrationBuilder.DropTable(
                name: "PushNotifications");

            migrationBuilder.DropTable(
                name: "SingleTasks");

            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "Webhooks_Inbound");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "RecurrentTasks");

            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
