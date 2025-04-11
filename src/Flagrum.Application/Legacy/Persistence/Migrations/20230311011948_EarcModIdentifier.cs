using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Application.Persistence.Migrations
{
    public partial class EarcModIdentifier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Flags",
                table: "EarcMods",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "Identifier",
                table: "EarcMods",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flags",
                table: "EarcMods");

            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "EarcMods");
        }
    }
}
