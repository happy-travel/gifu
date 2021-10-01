using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Gifu.Data.Migrations
{
    public partial class ChangePrimaryKeyHistoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AmountChangesHistories",
                table: "AmountChangesHistories");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AmountChangesHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AmountChangesHistories",
                table: "AmountChangesHistories",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AmountChangesHistories",
                table: "AmountChangesHistories");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AmountChangesHistories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AmountChangesHistories",
                table: "AmountChangesHistories",
                column: "VccId");
        }
    }
}
