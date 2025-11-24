using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GAIA.Infra.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class FixSeeder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FrameworkNode",
                schema: "ef-core");

            migrationBuilder.DropColumn(
                name: "Root_Content",
                schema: "ef-core",
                table: "Frameworks");

            migrationBuilder.DropColumn(
                name: "Root_Id",
                schema: "ef-core",
                table: "Frameworks");

            migrationBuilder.CreateTable(
                name: "FrameworkRoots",
                schema: "ef-core",
                columns: table => new
                {
                    FrameworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameworkRoots", x => x.FrameworkId);
                    table.ForeignKey(
                        name: "FK_FrameworkRoots_Frameworks_FrameworkId",
                        column: x => x.FrameworkId,
                        principalSchema: "ef-core",
                        principalTable: "Frameworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FrameworkRootChildren",
                schema: "ef-core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RootId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameworkRootChildren", x => new { x.RootId, x.Id });
                    table.ForeignKey(
                        name: "FK_FrameworkRootChildren_FrameworkRoots_RootId",
                        column: x => x.RootId,
                        principalSchema: "ef-core",
                        principalTable: "FrameworkRoots",
                        principalColumn: "FrameworkId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FrameworkRootChildren",
                schema: "ef-core");

            migrationBuilder.DropTable(
                name: "FrameworkRoots",
                schema: "ef-core");

            migrationBuilder.AddColumn<string>(
                name: "Root_Content",
                schema: "ef-core",
                table: "Frameworks",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Root_Id",
                schema: "ef-core",
                table: "Frameworks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "FrameworkNode",
                schema: "ef-core",
                columns: table => new
                {
                    FrameworkNodeFrameworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameworkNode", x => new { x.FrameworkNodeFrameworkId, x.Id });
                    table.ForeignKey(
                        name: "FK_FrameworkNode_Frameworks_FrameworkNodeFrameworkId",
                        column: x => x.FrameworkNodeFrameworkId,
                        principalSchema: "ef-core",
                        principalTable: "Frameworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
