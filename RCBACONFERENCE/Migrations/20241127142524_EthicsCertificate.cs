using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RCBACONFERENCE.Migrations
{
    /// <inheritdoc />
    public partial class EthicsCertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CONF_EthicsCertificate",
                columns: table => new
                {
                    EthicsID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResearchEventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Authors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EthicsCertficate = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateSubmitted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONF_EthicsCertificate", x => x.EthicsID);
                    table.ForeignKey(
                        name: "FK_CONF_EthicsCertificate_CONF_ResearchEvent_ResearchEventId",
                        column: x => x.ResearchEventId,
                        principalTable: "CONF_ResearchEvent",
                        principalColumn: "ResearchEventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONF_EthicsCertificate_CONF_UserConference_UserId",
                        column: x => x.UserId,
                        principalTable: "CONF_UserConference",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONF_EthicsCertificate_ResearchEventId",
                table: "CONF_EthicsCertificate",
                column: "ResearchEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_EthicsCertificate_UserId",
                table: "CONF_EthicsCertificate",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONF_EthicsCertificate");
        }
    }
}
