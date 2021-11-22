using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Gifu.Data.Migrations
{
    public partial class AddVccSupplierColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Supplier",
                table: "VccIssues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"UPDATE ""VccIssues"" SET ""Supplier"" = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "VccIssues");
        }
    }
}
