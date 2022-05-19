using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Gifu.Data.Migrations
{
    public partial class AddDateColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClearanceDate",
                table: "IxarisScheduleLoads",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ScheduleDate",
                table: "IxarisScheduleLoads",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClearanceDate",
                table: "IxarisScheduleLoads");

            migrationBuilder.DropColumn(
                name: "ScheduleDate",
                table: "IxarisScheduleLoads");
        }
    }
}
