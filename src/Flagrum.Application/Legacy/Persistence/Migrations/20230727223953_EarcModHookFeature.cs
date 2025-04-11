using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EarcModHookFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EarcModHookFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EarcModId = table.Column<int>(type: "INTEGER", nullable: false),
                    Feature = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarcModHookFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EarcModHookFeatures_EarcMods_EarcModId",
                        column: x => x.EarcModId,
                        principalTable: "EarcMods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EarcModHookFeatures_EarcModId",
                table: "EarcModHookFeatures",
                column: "EarcModId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EarcModHookFeatures");
        }
    }
}
