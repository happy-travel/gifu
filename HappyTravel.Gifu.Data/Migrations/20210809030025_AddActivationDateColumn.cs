using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Gifu.Data.Migrations
{
    public partial class AddActivationDateColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationDate",
                table: "VccIssues",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivationDate",
                table: "VccIssues");
        }
    }
}
