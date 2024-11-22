using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RCBACONFERENCE.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "CONF_EvaluationTable",
                newName: "TechnicalQuality");

            migrationBuilder.AddColumn<string>(
                name: "ResearchEventId",
                table: "CONF_UploadPapers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ClarityPresentation",
                table: "CONF_EvaluationTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepthResearch",
                table: "CONF_EvaluationTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OriginalityApproach",
                table: "CONF_EvaluationTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RelevanceTheme",
                table: "CONF_EvaluationTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScientificNovelty",
                table: "CONF_EvaluationTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SignificanceContribution",
                table: "CONF_EvaluationTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_CONF_UploadPapers_ResearchEventId",
                table: "CONF_UploadPapers",
                column: "ResearchEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_Registration_ResearchEventId",
                table: "CONF_Registration",
                column: "ResearchEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_Registration_UserId",
                table: "CONF_Registration",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONF_UploadPapers_CONF_ResearchEvent_ResearchEventId",
                table: "CONF_UploadPapers",
                column: "ResearchEventId",
                principalTable: "CONF_ResearchEvent",
                principalColumn: "ResearchEventId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONF_UploadPapers_CONF_ResearchEvent_ResearchEventId",
                table: "CONF_UploadPapers");

            migrationBuilder.DropTable(
                name: "CONF_Registration");

            migrationBuilder.DropIndex(
                name: "IX_CONF_UploadPapers_ResearchEventId",
                table: "CONF_UploadPapers");

            migrationBuilder.DropColumn(
                name: "ResearchEventId",
                table: "CONF_UploadPapers");

            migrationBuilder.DropColumn(
                name: "ClarityPresentation",
                table: "CONF_EvaluationTable");

            migrationBuilder.DropColumn(
                name: "DepthResearch",
                table: "CONF_EvaluationTable");

            migrationBuilder.DropColumn(
                name: "OriginalityApproach",
                table: "CONF_EvaluationTable");

            migrationBuilder.DropColumn(
                name: "RelevanceTheme",
                table: "CONF_EvaluationTable");

            migrationBuilder.DropColumn(
                name: "ScientificNovelty",
                table: "CONF_EvaluationTable");

            migrationBuilder.DropColumn(
                name: "SignificanceContribution",
                table: "CONF_EvaluationTable");

            migrationBuilder.RenameColumn(
                name: "TechnicalQuality",
                table: "CONF_EvaluationTable",
                newName: "Rating");
        }
    }
}
