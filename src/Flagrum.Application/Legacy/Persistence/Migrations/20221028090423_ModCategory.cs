using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Application.Persistence.Migrations
{
    public partial class ModCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "EarcMods",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFavourite",
                table: "EarcMods",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "EarcMods");

            migrationBuilder.DropColumn(
                name: "IsFavourite",
                table: "EarcMods");
        }
    }
}
