using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Web.Persistence.Migrations
{
    public partial class Ps4AssetMap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ps4ArchiveLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ps4ArchiveLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ps4AssetUris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uri = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ps4AssetUris", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ps4ArchiveAssets",
                columns: table => new
                {
                    Ps4ArchiveLocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ps4AssetUriId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ps4ArchiveAssets", x => new { x.Ps4ArchiveLocationId, x.Ps4AssetUriId });
                    table.ForeignKey(
                        name: "FK_Ps4ArchiveAssets_Ps4ArchiveLocations_Ps4ArchiveLocationId",
                        column: x => x.Ps4ArchiveLocationId,
                        principalTable: "Ps4ArchiveLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ps4ArchiveAssets_Ps4AssetUris_Ps4AssetUriId",
                        column: x => x.Ps4AssetUriId,
                        principalTable: "Ps4AssetUris",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ps4ArchiveAssets_Ps4AssetUriId",
                table: "Ps4ArchiveAssets",
                column: "Ps4AssetUriId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ps4ArchiveAssets");

            migrationBuilder.DropTable(
                name: "Ps4ArchiveLocations");

            migrationBuilder.DropTable(
                name: "Ps4AssetUris");
        }
    }
}
