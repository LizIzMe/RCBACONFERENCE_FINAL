using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RCBACONFERENCE.Migrations
{
    /// <inheritdoc />
    public partial class BoolEthics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresEthicsCertificate",
                table: "CONF_ResearchEvent",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresEthicsCertificate",
                table: "CONF_ResearchEvent");
        }
    }
}
