using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Gifu.Data.Migrations
{
    public partial class AddVccVendorColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VccVendor",
                table: "VccIssues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"UPDATE ""VccIssues"" SET ""VccVendor"" = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VccVendor",
                table: "VccIssues");
        }
    }
}
