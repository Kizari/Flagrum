using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Application.Persistence.Migrations
{
    public partial class EarcModFileLastModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileLastModified",
                table: "EarcModReplacements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FileLastModified",
                table: "EarcModLooseFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileLastModified",
                table: "EarcModReplacements");

            migrationBuilder.DropColumn(
                name: "FileLastModified",
                table: "EarcModLooseFile");
        }
    }
}
