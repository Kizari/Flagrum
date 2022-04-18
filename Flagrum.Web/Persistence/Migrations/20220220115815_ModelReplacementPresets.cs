using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Web.Persistence.Migrations
{
    public partial class ModelReplacementPresets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelReplacementFavourites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelReplacementFavourites", x => new { x.Id, x.IsDefault });
                });

            migrationBuilder.CreateTable(
                name: "ModelReplacementPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelReplacementPresets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModelReplacementPaths",
                columns: table => new
                {
                    ModelReplacementPresetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelReplacementPaths", x => new { x.ModelReplacementPresetId, x.Path });
                    table.ForeignKey(
                        name: "FK_ModelReplacementPaths_ModelReplacementPresets_ModelReplacementPresetId",
                        column: x => x.ModelReplacementPresetId,
                        principalTable: "ModelReplacementPresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelReplacementFavourites");

            migrationBuilder.DropTable(
                name: "ModelReplacementPaths");

            migrationBuilder.DropTable(
                name: "ModelReplacementPresets");
        }
    }
}
