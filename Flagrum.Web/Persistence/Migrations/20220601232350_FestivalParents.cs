using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Web.Persistence.Migrations
{
    public partial class FestivalParents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
