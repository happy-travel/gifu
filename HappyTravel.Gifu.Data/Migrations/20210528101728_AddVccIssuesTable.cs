using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Gifu.Data.Migrations
{
    public partial class AddVccIssuesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VccIssues",
                columns: table => new
                {
                    TransactionId = table.Column<string>(type: "text", nullable: false),
                    ReferenceCode = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VccIssues", x => x.TransactionId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VccIssues");
        }
    }
}
