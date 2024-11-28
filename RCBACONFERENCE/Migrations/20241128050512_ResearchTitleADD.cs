using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RCBACONFERENCE.Migrations
{
    /// <inheritdoc />
    public partial class ResearchTitleADD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResearchTitle",
                table: "CONF_EthicsCertificate",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResearchTitle",
                table: "CONF_EthicsCertificate");
        }
    }
}
