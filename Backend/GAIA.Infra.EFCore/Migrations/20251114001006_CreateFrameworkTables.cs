using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GAIA.Infra.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class CreateFrameworkTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ef-core");

            migrationBuilder.CreateTable(
                name: "Frameworks",
                schema: "ef-core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Root_Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Root_Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frameworks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FrameworkDepth",
                schema: "ef-core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Depth = table.Column<int>(type: "integer", nullable: false),
                    FrameworkId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameworkDepth", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FrameworkDepth_Frameworks_FrameworkId",
                        column: x => x.FrameworkId,
                        principalSchema: "ef-core",
                        principalTable: "Frameworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FrameworkNode",
                schema: "ef-core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FrameworkNodeFrameworkId = table.Column<Guid>(type: "uuid", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Scoring",
                schema: "ef-core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FrameworkId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scoring", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scoring_Frameworks_FrameworkId",
                        column: x => x.FrameworkId,
                        principalSchema: "ef-core",
                        principalTable: "Frameworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FrameworkDepth_Depth_Name",
                schema: "ef-core",
                table: "FrameworkDepth",
                columns: new[] { "Depth", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_FrameworkDepth_FrameworkId",
                schema: "ef-core",
                table: "FrameworkDepth",
                column: "FrameworkId");

            migrationBuilder.CreateIndex(
                name: "IX_Scoring_FrameworkId",
                schema: "ef-core",
                table: "Scoring",
                column: "FrameworkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FrameworkDepth",
                schema: "ef-core");

            migrationBuilder.DropTable(
                name: "FrameworkNode",
                schema: "ef-core");

            migrationBuilder.DropTable(
                name: "Scoring",
                schema: "ef-core");

            migrationBuilder.DropTable(
                name: "Frameworks",
                schema: "ef-core");
        }
    }
}
