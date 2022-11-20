using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Web.Persistence.Migrations
{
    public partial class EarcModFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Flags",
                table: "EarcModReplacements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Flags",
                table: "EarcModEarcs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flags",
                table: "EarcModReplacements");

            migrationBuilder.DropColumn(
                name: "Flags",
                table: "EarcModEarcs");
        }
    }
}
