using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Web.Persistence.Migrations
{
    public partial class EarcMods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EarcMods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarcMods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EarcModEarcs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EarcModId = table.Column<int>(type: "INTEGER", nullable: false),
                    EarcRelativePath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarcModEarcs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EarcModEarcs_EarcMods_EarcModId",
                        column: x => x.EarcModId,
                        principalTable: "EarcMods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EarcModReplacements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EarcModEarcId = table.Column<int>(type: "INTEGER", nullable: false),
                    Uri = table.Column<string>(type: "TEXT", nullable: true),
                    ReplacementFilePath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarcModReplacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EarcModReplacements_EarcModEarcs_EarcModEarcId",
                        column: x => x.EarcModEarcId,
                        principalTable: "EarcModEarcs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EarcModEarcs_EarcModId",
                table: "EarcModEarcs",
                column: "EarcModId");

            migrationBuilder.CreateIndex(
                name: "IX_EarcModReplacements_EarcModEarcId",
                table: "EarcModReplacements",
                column: "EarcModEarcId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EarcModReplacements");

            migrationBuilder.DropTable(
                name: "EarcModEarcs");

            migrationBuilder.DropTable(
                name: "EarcMods");
        }
    }
}
