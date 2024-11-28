using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RCBACONFERENCE.Migrations
{
    /// <inheritdoc />
    public partial class NewUploadPaperInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CONF_UploadPapers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EthicsID",
                table: "CONF_UploadPapers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CONF_UploadPapers_EthicsID",
                table: "CONF_UploadPapers",
                column: "EthicsID");

            migrationBuilder.AddForeignKey(
                name: "FK_CONF_UploadPapers_CONF_EthicsCertificate_EthicsID",
                table: "CONF_UploadPapers",
                column: "EthicsID",
                principalTable: "CONF_EthicsCertificate",
                principalColumn: "EthicsID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONF_UploadPapers_CONF_EthicsCertificate_EthicsID",
                table: "CONF_UploadPapers");

            migrationBuilder.DropIndex(
                name: "IX_CONF_UploadPapers_EthicsID",
                table: "CONF_UploadPapers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CONF_UploadPapers");

            migrationBuilder.DropColumn(
                name: "EthicsID",
                table: "CONF_UploadPapers");
        }
    }
}
