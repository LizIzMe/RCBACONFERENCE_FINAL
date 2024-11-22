using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RCBACONFERENCE.Migrations
{
    /// <inheritdoc />
    public partial class ReciptTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_CONF_Receipt_ResearchEventId",
                table: "CONF_Receipt",
                column: "ResearchEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CONF_Receipt_UserId",
                table: "CONF_Receipt",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONF_Receipt");
        }
    }
}
