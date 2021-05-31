using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Gifu.Data.Migrations
{
    public partial class AddClientIdToVccIssuesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "VccIssues",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "VccIssues");
        }
    }
}
