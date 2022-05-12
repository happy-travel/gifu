using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Gifu.Data.Migrations
{
    public partial class AddIssuedMoneyAmountColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "IssuedAmount",
                table: "VccIssues",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "IssuedCurrency",
                table: "VccIssues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"Update ""VccIssues"" SET ""IssuedAmount"" = ""Amount"", ""IssuedCurrency"" = ""Currency""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssuedAmount",
                table: "VccIssues");

            migrationBuilder.DropColumn(
                name: "IssuedCurrency",
                table: "VccIssues");
        }
    }
}
