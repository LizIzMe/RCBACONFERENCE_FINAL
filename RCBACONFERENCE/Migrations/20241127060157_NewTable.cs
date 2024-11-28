using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RCBACONFERENCE.Migrations
{
    /// <inheritdoc />
    public partial class NewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CONF_ConferenceRoles",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_ConferenceRoles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "CONF_ResearchEvent",
                columns: table => new
                {
                    ResearchEventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventThumbnail = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    RegistrationOpen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistrationDeadline = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_ResearchEvent", x => x.ResearchEventId);
                });

            migrationBuilder.CreateTable(
                name: "CONF_UserConference",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Affiliation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryRegion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerificationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_UserConference", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "CONF_ScheduleEvent",
                columns: table => new
                {
                    ScheduleEventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResearchEventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_ScheduleEvent", x => x.ScheduleEventId);
                    table.ForeignKey(
                        name: "FK_CONF_ScheduleEvent_CONF_ResearchEvent_ResearchEventId",
                        column: x => x.ResearchEventId,
                        principalTable: "CONF_ResearchEvent",
                        principalColumn: "ResearchEventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CONF_Receipt",
                columns: table => new
                {
                    ReceiptId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResearchEventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiptFile = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_Receipt", x => x.ReceiptId);
                    table.ForeignKey(
                        name: "FK_CONF_Receipt_CONF_ResearchEvent_ResearchEventId",
                        column: x => x.ResearchEventId,
                        principalTable: "CONF_ResearchEvent",
                        principalColumn: "ResearchEventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONF_Receipt_CONF_UserConference_UserId",
                        column: x => x.UserId,
                        principalTable: "CONF_UserConference",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CONF_Registration",
                columns: table => new
                {
                    RegistrationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResearchEventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_Registration", x => x.RegistrationId);
                    table.ForeignKey(
                        name: "FK_CONF_Registration_CONF_ResearchEvent_ResearchEventId",
                        column: x => x.ResearchEventId,
                        principalTable: "CONF_ResearchEvent",
                        principalColumn: "ResearchEventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONF_Registration_CONF_UserConference_UserId",
                        column: x => x.UserId,
                        principalTable: "CONF_UserConference",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CONF_UserConferenceRoles",
                columns: table => new
                {
                    UserRoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_UserConferenceRoles", x => x.UserRoleId);
                    table.ForeignKey(
                        name: "FK_CONF_UserConferenceRoles_CONF_ConferenceRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "CONF_ConferenceRoles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONF_UserConferenceRoles_CONF_UserConference_UserId",
                        column: x => x.UserId,
                        principalTable: "CONF_UserConference",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CONF_EvaluatorTable",
                columns: table => new
                {
                    EvaluatorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserRoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_EvaluatorTable", x => x.EvaluatorId);
                    table.ForeignKey(
                        name: "FK_CONF_EvaluatorTable_CONF_UserConferenceRoles_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "CONF_UserConferenceRoles",
                        principalColumn: "UserRoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CONF_UploadPapers",
                columns: table => new
                {
                    UploadPaperID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserRoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResearchEventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Abstract = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Affiliation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Authors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_UploadPapers", x => x.UploadPaperID);
                    table.ForeignKey(
                        name: "FK_CONF_UploadPapers_CONF_ResearchEvent_ResearchEventId",
                        column: x => x.ResearchEventId,
                        principalTable: "CONF_ResearchEvent",
                        principalColumn: "ResearchEventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONF_UploadPapers_CONF_UserConferenceRoles_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "CONF_UserConferenceRoles",
                        principalColumn: "UserRoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONF_UploadPapers_CONF_UserConference_UserId",
                        column: x => x.UserId,
                        principalTable: "CONF_UserConference",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CONF_EvaluationTable",
                columns: table => new
                {
                    EvaluationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UploadPaperID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EvaluatorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScientificNovelty = table.Column<int>(type: "int", nullable: false),
                    SignificanceContribution = table.Column<int>(type: "int", nullable: false),
                    TechnicalQuality = table.Column<int>(type: "int", nullable: false),
                    DepthResearch = table.Column<int>(type: "int", nullable: false),
                    ClarityPresentation = table.Column<int>(type: "int", nullable: false),
                    RelevanceTheme = table.Column<int>(type: "int", nullable: false),
                    OriginalityApproach = table.Column<int>(type: "int", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_EvaluationTable", x => x.EvaluationId);
                    table.ForeignKey(
                        name: "FK_CONF_EvaluationTable_CONF_EvaluatorTable_EvaluatorId",
                        column: x => x.EvaluatorId,
                        principalTable: "CONF_EvaluatorTable",
                        principalColumn: "EvaluatorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONF_EvaluationTable_CONF_UploadPapers_UploadPaperID",
                        column: x => x.UploadPaperID,
                        principalTable: "CONF_UploadPapers",
                        principalColumn: "UploadPaperID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CONF_PaperAssignmentTable",
                columns: table => new
                {
                    AssignmentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UploadPaperID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EvaluatorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_PaperAssignmentTable", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_CONF_PaperAssignmentTable_CONF_EvaluatorTable_EvaluatorId",
                        column: x => x.EvaluatorId,
                        principalTable: "CONF_EvaluatorTable",
                        principalColumn: "EvaluatorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONF_PaperAssignmentTable_CONF_UploadPapers_UploadPaperID",
                        column: x => x.UploadPaperID,
                        principalTable: "CONF_UploadPapers",
                        principalColumn: "UploadPaperID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONF_EvaluationTable_EvaluatorId",
                table: "CONF_EvaluationTable",
                column: "EvaluatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_EvaluationTable_UploadPaperID",
                table: "CONF_EvaluationTable",
                column: "UploadPaperID");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_EvaluatorTable_UserRoleId",
                table: "CONF_EvaluatorTable",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_PaperAssignmentTable_EvaluatorId",
                table: "CONF_PaperAssignmentTable",
                column: "EvaluatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_PaperAssignmentTable_UploadPaperID",
                table: "CONF_PaperAssignmentTable",
                column: "UploadPaperID");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_Receipt_ResearchEventId",
                table: "CONF_Receipt",
                column: "ResearchEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_Receipt_UserId",
                table: "CONF_Receipt",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_Registration_ResearchEventId",
                table: "CONF_Registration",
                column: "ResearchEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_Registration_UserId",
                table: "CONF_Registration",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_ScheduleEvent_ResearchEventId",
                table: "CONF_ScheduleEvent",
                column: "ResearchEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_UploadPapers_ResearchEventId",
                table: "CONF_UploadPapers",
                column: "ResearchEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_UploadPapers_UserId",
                table: "CONF_UploadPapers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_UploadPapers_UserRoleId",
                table: "CONF_UploadPapers",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_UserConferenceRoles_RoleId",
                table: "CONF_UserConferenceRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_UserConferenceRoles_UserId",
                table: "CONF_UserConferenceRoles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONF_EvaluationTable");

            migrationBuilder.DropTable(
                name: "CONF_PaperAssignmentTable");

            migrationBuilder.DropTable(
                name: "CONF_Receipt");

            migrationBuilder.DropTable(
                name: "CONF_Registration");

            migrationBuilder.DropTable(
                name: "CONF_ScheduleEvent");

            migrationBuilder.DropTable(
                name: "CONF_EvaluatorTable");

            migrationBuilder.DropTable(
                name: "CONF_UploadPapers");

            migrationBuilder.DropTable(
                name: "CONF_ResearchEvent");

            migrationBuilder.DropTable(
                name: "CONF_UserConferenceRoles");

            migrationBuilder.DropTable(
                name: "CONF_ConferenceRoles");

            migrationBuilder.DropTable(
                name: "CONF_UserConference");
        }
    }
}
