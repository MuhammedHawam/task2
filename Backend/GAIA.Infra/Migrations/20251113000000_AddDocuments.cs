using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GAIA.Infra.Migrations;

public partial class AddDocuments : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.Sql("DROP TABLE IF EXISTS \"Assessments\";");

    migrationBuilder.CreateTable(
      name: "Documents",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        Content = table.Column<byte[]>(type: "bytea", nullable: false),
        Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
        Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
        Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_Documents", x => x.Id);
      });
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
      name: "Documents");

    migrationBuilder.CreateTable(
      name: "Assessments",
      columns: table => new
      {
        Id = table.Column<int>(type: "integer", nullable: false)
          .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
        Name = table.Column<string>(type: "text", nullable: false),
        Status = table.Column<string>(type: "text", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_Assessments", x => x.Id);
      });
  }
}
