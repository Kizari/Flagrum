using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Web.Persistence.Migrations
{
    public partial class DependenciesManyToMany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FestivalDependencies_FestivalDependencies_ParentId",
                table: "FestivalDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_FestivalMaterialDependencies_FestivalModelDependencies_ParentId",
                table: "FestivalMaterialDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_FestivalModelDependencies_FestivalSubdependencies_ParentId",
                table: "FestivalModelDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_FestivalSubdependencies_FestivalDependencies_ParentId",
                table: "FestivalSubdependencies");

            migrationBuilder.DropTable(
                name: "FestivalAllDependencies");

            migrationBuilder.DropTable(
                name: "FestivalFinalDependencies");

            migrationBuilder.DropIndex(
                name: "IX_FestivalSubdependencies_ParentId",
                table: "FestivalSubdependencies");

            migrationBuilder.DropIndex(
                name: "IX_FestivalModelDependencies_ParentId",
                table: "FestivalModelDependencies");

            migrationBuilder.DropIndex(
                name: "IX_FestivalMaterialDependencies_ParentId",
                table: "FestivalMaterialDependencies");

            migrationBuilder.DropIndex(
                name: "IX_FestivalDependencies_ParentId",
                table: "FestivalDependencies");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "FestivalSubdependencies");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "FestivalModelDependencies");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "FestivalMaterialDependencies");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "FestivalDependencies");

            migrationBuilder.CreateTable(
                name: "FestivalDependencyFestivalDependency",
                columns: table => new
                {
                    ParentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChildId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FestivalDependencyFestivalDependency", x => new { x.ParentId, x.ChildId });
                    table.ForeignKey(
                        name: "FK_FestivalDependencyFestivalDependency_FestivalDependencies_ChildId",
                        column: x => x.ChildId,
                        principalTable: "FestivalDependencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FestivalDependencyFestivalDependency_FestivalDependencies_ParentId",
                        column: x => x.ParentId,
                        principalTable: "FestivalDependencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FestivalDependencyFestivalSubdependency",
                columns: table => new
                {
                    DependencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubdependencyId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FestivalDependencyFestivalSubdependency", x => new { x.DependencyId, x.SubdependencyId });
                    table.ForeignKey(
                        name: "FK_FestivalDependencyFestivalSubdependency_FestivalDependencies_DependencyId",
                        column: x => x.DependencyId,
                        principalTable: "FestivalDependencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FestivalDependencyFestivalSubdependency_FestivalSubdependencies_SubdependencyId",
                        column: x => x.SubdependencyId,
                        principalTable: "FestivalSubdependencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FestivalModelDependencyFestivalMaterialDependency",
                columns: table => new
                {
                    ModelDependencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialDependencyId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FestivalModelDependencyFestivalMaterialDependency", x => new { x.ModelDependencyId, x.MaterialDependencyId });
                    table.ForeignKey(
                        name: "FK_FestivalModelDependencyFestivalMaterialDependency_FestivalMaterialDependencies_MaterialDependencyId",
                        column: x => x.MaterialDependencyId,
                        principalTable: "FestivalMaterialDependencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FestivalModelDependencyFestivalMaterialDependency_FestivalModelDependencies_ModelDependencyId",
                        column: x => x.ModelDependencyId,
                        principalTable: "FestivalModelDependencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FestivalSubdependencyFestivalModelDependency",
                columns: table => new
                {
                    SubdependencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModelDependencyId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FestivalSubdependencyFestivalModelDependency", x => new { x.SubdependencyId, x.ModelDependencyId });
                    table.ForeignKey(
                        name: "FK_FestivalSubdependencyFestivalModelDependency_FestivalModelDependencies_ModelDependencyId",
                        column: x => x.ModelDependencyId,
                        principalTable: "FestivalModelDependencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FestivalSubdependencyFestivalModelDependency_FestivalSubdependencies_SubdependencyId",
                        column: x => x.SubdependencyId,
                        principalTable: "FestivalSubdependencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FestivalDependencyFestivalDependency_ChildId",
                table: "FestivalDependencyFestivalDependency",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_FestivalDependencyFestivalSubdependency_SubdependencyId",
                table: "FestivalDependencyFestivalSubdependency",
                column: "SubdependencyId");

            migrationBuilder.CreateIndex(
                name: "IX_FestivalModelDependencyFestivalMaterialDependency_MaterialDependencyId",
                table: "FestivalModelDependencyFestivalMaterialDependency",
                column: "MaterialDependencyId");

            migrationBuilder.CreateIndex(
                name: "IX_FestivalSubdependencyFestivalModelDependency_ModelDependencyId",
                table: "FestivalSubdependencyFestivalModelDependency",
                column: "ModelDependencyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FestivalDependencyFestivalDependency");

            migrationBuilder.DropTable(
                name: "FestivalDependencyFestivalSubdependency");

            migrationBuilder.DropTable(
                name: "FestivalModelDependencyFestivalMaterialDependency");

            migrationBuilder.DropTable(
                name: "FestivalSubdependencyFestivalModelDependency");

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "FestivalSubdependencies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "FestivalModelDependencies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "FestivalMaterialDependencies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "FestivalDependencies",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FestivalAllDependencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uri = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FestivalAllDependencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FestivalFinalDependencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uri = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FestivalFinalDependencies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FestivalSubdependencies_ParentId",
                table: "FestivalSubdependencies",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FestivalModelDependencies_ParentId",
                table: "FestivalModelDependencies",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FestivalMaterialDependencies_ParentId",
                table: "FestivalMaterialDependencies",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FestivalDependencies_ParentId",
                table: "FestivalDependencies",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_FestivalDependencies_FestivalDependencies_ParentId",
                table: "FestivalDependencies",
                column: "ParentId",
                principalTable: "FestivalDependencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FestivalMaterialDependencies_FestivalModelDependencies_ParentId",
                table: "FestivalMaterialDependencies",
                column: "ParentId",
                principalTable: "FestivalModelDependencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FestivalModelDependencies_FestivalSubdependencies_ParentId",
                table: "FestivalModelDependencies",
                column: "ParentId",
                principalTable: "FestivalSubdependencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FestivalSubdependencies_FestivalDependencies_ParentId",
                table: "FestivalSubdependencies",
                column: "ParentId",
                principalTable: "FestivalDependencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
