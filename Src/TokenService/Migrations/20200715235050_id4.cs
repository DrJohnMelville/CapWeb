 using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TokenService.Migrations
{
    public partial class id4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConsumedTime",
                table: "PersistedGrants",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PersistedGrants",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "PersistedGrants",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumedTime",
                table: "PersistedGrants");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PersistedGrants");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "PersistedGrants");
        }
    }
}
