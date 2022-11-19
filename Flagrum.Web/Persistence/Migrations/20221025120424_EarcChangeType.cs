using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Web.Persistence.Migrations
{
    public partial class EarcChangeType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "EarcModEarcs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "EarcModEarcs");
        }
    }
}
