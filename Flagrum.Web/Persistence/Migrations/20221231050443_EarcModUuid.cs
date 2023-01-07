using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flagrum.Web.Persistence.Migrations
{
    public partial class EarcModUuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear indices
            migrationBuilder.DropIndex(
                name: "IX_EarcModEarcs_EarcModId",
                table: "EarcModEarcs");
            
            migrationBuilder.DropIndex(
                name: "IX_EarcModLooseFile_EarcModId",
                table: "EarcModLooseFile");
            
            migrationBuilder.DropIndex(
                name: "IX_EarcModReplacements_EarcModEarcId",
                table: "EarcModReplacements");

            // Move Ids to OldIds and remake Id as string
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "EarcMods",
                newName: "OldId");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "EarcMods",
                type: "TEXT",
                nullable: false,
                defaultValue: "MIGRATION");
            
            // Move EarcModIds to OldEarcModIds and remake EarcModId as string
            migrationBuilder.RenameColumn(
                name: "EarcModId",
                table: "EarcModEarcs",
                newName: "OldEarcModId");
            
            migrationBuilder.AddColumn<string>(
                name: "EarcModId",
                table: "EarcModEarcs",
                type: "TEXT",
                nullable: true);
            
            // Move EarcModIds to OldEarcModIds and remake EarcModId as string
            migrationBuilder.RenameColumn(
                name: "EarcModId",
                table: "EarcModLooseFile",
                newName: "OldEarcModId");
            
            migrationBuilder.AddColumn<string>(
                name: "EarcModId",
                table: "EarcModLooseFile",
                type: "TEXT",
                nullable: true);

            // Manual SQL to populate new string based Ids
            migrationBuilder.Sql("UPDATE EarcMods SET Id = uuid();");
            migrationBuilder.Sql(
                "UPDATE EarcModEarcs SET EarcModId = m.Id FROM (SELECT Id, OldId FROM EarcMods) AS m WHERE m.OldId = EarcModEarcs.OldEarcModId;");
            migrationBuilder.Sql(
                "UPDATE EarcModLooseFile SET EarcModId = m.Id FROM (SELECT Id, OldId FROM EarcMods) AS m WHERE m.OldId = EarcModLooseFile.OldEarcModId;");
            
            // Recreate tables (SQLite can't alter constraints so it must be done this way)
            migrationBuilder.CreateTable(
                name: "ef_temp_EarcMods",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFavourite = table.Column<bool>(type: "INTEGER", nullable: false),
                    PrerequisiteId = table.Column<string>(type: "TEXT", nullable: true),
                    Readme = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarcMods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EarcMods_EarcMods_PrerequisiteId",
                        column: x => x.PrerequisiteId,
                        principalTable: "ef_temp_EarcMods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            migrationBuilder.CreateTable(
                name: "ef_temp_EarcModEarcs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EarcModId = table.Column<int>(type: "INTEGER", nullable: false),
                    EarcRelativePath = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Flags = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarcModEarcs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EarcModEarcs_EarcMods_EarcModId",
                        column: x => x.EarcModId,
                        principalTable: "ef_temp_EarcMods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            migrationBuilder.CreateTable(
                name: "ef_temp_EarcModLooseFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EarcModId = table.Column<int>(type: "INTEGER", nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    FileLastModified = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarcModLooseFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EarcModLooseFiles_EarcMods_EarcModId",
                        column: x => x.EarcModId,
                        principalTable: "ef_temp_EarcMods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            migrationBuilder.CreateTable(
                name: "EarcModFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EarcModEarcId = table.Column<int>(type: "INTEGER", nullable: false),
                    Uri = table.Column<string>(type: "TEXT", nullable: true),
                    ReplacementFilePath = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Flags = table.Column<int>(type: "INTEGER", nullable: false),
                    FileLastModified = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarcModFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EarcModFiles_EarcModEarcs_EarcModEarcId",
                        column: x => x.EarcModEarcId,
                        principalTable: "ef_temp_EarcModEarcs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            // Copy the data to the temp tables
            migrationBuilder.Sql(
@"INSERT INTO ""ef_temp_EarcMods"" (""Id"", ""Author"", ""Category"", ""Description"", ""IsActive"", ""IsFavourite"", ""Name"", ""Readme"")
SELECT ""Id"", ""Author"", ""Category"", ""Description"", ""IsActive"", ""IsFavourite"", ""Name"", ""Readme""
FROM ""EarcMods"";");

            migrationBuilder.Sql(
@"INSERT INTO ""ef_temp_EarcModEarcs"" (""Id"", ""EarcModId"", ""EarcRelativePath"", ""Flags"", ""Type"")
SELECT ""Id"", ""EarcModId"", ""EarcRelativePath"", ""Flags"", ""Type""
FROM ""EarcModEarcs"";
");

            migrationBuilder.Sql(
@"INSERT INTO ""ef_temp_EarcModLooseFiles"" (""Id"", ""EarcModId"", ""FileLastModified"", ""FilePath"", ""RelativePath"", ""Type"")
SELECT ""Id"", ""EarcModId"", ""FileLastModified"", ""FilePath"", ""RelativePath"", ""Type""
FROM ""EarcModLooseFile"";
");

            migrationBuilder.Sql(
@"INSERT INTO EarcModFiles (Id, EarcModEarcId, Uri, ReplacementFilePath, Type, Flags, FileLastModified)
SELECT Id, EarcModEarcId, Uri, ReplacementFilePath, Type, Flags, FileLastModified
FROM EarcModReplacements;");

            // Replace tables with their temp variants
            migrationBuilder.DropTable("EarcMods");
            migrationBuilder.RenameTable(
                name: "ef_temp_EarcMods",
                newName: "EarcMods");
            
            migrationBuilder.DropTable("EarcModEarcs");
            migrationBuilder.RenameTable(
                name: "ef_temp_EarcModEarcs",
                newName: "EarcModEarcs");
            
            migrationBuilder.DropTable("EarcModLooseFile");
            migrationBuilder.RenameTable(
                name: "ef_temp_EarcModLooseFiles",
                newName: "EarcModLooseFiles");

            migrationBuilder.DropTable("EarcModReplacements");
            
            // Recreate indices
            migrationBuilder.CreateIndex(
                name: "IX_EarcModEarcs_EarcModId",
                table: "EarcModEarcs",
                column: "EarcModId");
            
            migrationBuilder.CreateIndex(
                name: "IX_EarcModLooseFile_EarcModId",
                table: "EarcModLooseFiles",
                column: "EarcModId");
            
            migrationBuilder.CreateIndex(
                name: "IX_EarcMods_PrerequisiteId",
                table: "EarcMods",
                column: "PrerequisiteId");
            
            migrationBuilder.CreateIndex(
                name: "IX_EarcModFiles_EarcModEarcId",
                table: "EarcModFiles",
                column: "EarcModEarcId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EarcModEarcs_EarcMods_EarcModId",
                table: "EarcModEarcs");

            migrationBuilder.DropForeignKey(
                name: "FK_EarcModLooseFile_EarcMods_EarcModId",
                table: "EarcModLooseFile");

            migrationBuilder.DropForeignKey(
                name: "FK_EarcMods_EarcMods_PrerequisiteId",
                table: "EarcMods");

            migrationBuilder.DropIndex(
                name: "IX_EarcMods_PrerequisiteId",
                table: "EarcMods");

            migrationBuilder.DropColumn(
                name: "PrerequisiteId",
                table: "EarcMods");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "EarcMods",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "EarcModId",
                table: "EarcModLooseFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EarcModId",
                table: "EarcModEarcs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EarcModEarcs_EarcMods_EarcModId",
                table: "EarcModEarcs",
                column: "EarcModId",
                principalTable: "EarcMods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EarcModLooseFile_EarcMods_EarcModId",
                table: "EarcModLooseFile",
                column: "EarcModId",
                principalTable: "EarcMods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
