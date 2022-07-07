using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Web.Persistence.Migrations
{
    public partial class Ps4VertexLayoutTypeMap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VertexLayoutType",
                table: "FestivalModelDependencies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Ps4VertexLayoutTypeMaps",
                columns: table => new
                {
                    Uri = table.Column<string>(type: "TEXT", nullable: false),
                    VertexLayoutType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ps4VertexLayoutTypeMaps", x => x.Uri);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ps4VertexLayoutTypeMaps");

            migrationBuilder.DropColumn(
                name: "VertexLayoutType",
                table: "FestivalModelDependencies");
        }
    }
}
