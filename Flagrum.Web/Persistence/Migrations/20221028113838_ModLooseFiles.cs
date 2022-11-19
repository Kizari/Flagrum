using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Web.Persistence.Migrations
{
    public partial class ModLooseFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EarcModLooseFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EarcModId = table.Column<int>(type: "INTEGER", nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarcModLooseFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EarcModLooseFile_EarcMods_EarcModId",
                        column: x => x.EarcModId,
                        principalTable: "EarcMods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EarcModLooseFile_EarcModId",
                table: "EarcModLooseFile",
                column: "EarcModId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EarcModLooseFile");
        }
    }
}
