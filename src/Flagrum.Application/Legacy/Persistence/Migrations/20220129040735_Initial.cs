using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Application.Persistence.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchiveLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetExplorerNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetExplorerNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetExplorerNodes_AssetExplorerNodes_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AssetExplorerNodes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetUris",
                columns: table => new
                {
                    Uri = table.Column<string>(type: "TEXT", nullable: false),
                    ArchiveLocationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetUris", x => x.Uri);
                    table.ForeignKey(
                        name: "FK_AssetUris_ArchiveLocations_ArchiveLocationId",
                        column: x => x.ArchiveLocationId,
                        principalTable: "ArchiveLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetExplorerNodes_ParentId",
                table: "AssetExplorerNodes",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetUris_ArchiveLocationId",
                table: "AssetUris",
                column: "ArchiveLocationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetExplorerNodes");

            migrationBuilder.DropTable(
                name: "AssetUris");

            migrationBuilder.DropTable(
                name: "ArchiveLocations");
        }
    }
}
