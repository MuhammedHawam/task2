using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GAIA.Infra.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
        migrationBuilder.CreateTable(
            name: "Documents",
            columns: table => new
            {
              Id = table.Column<Guid>(type: "uuid", nullable: false),
              Content = table.Column<byte[]>(type: "bytea", nullable: false),
              Status = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
              Category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
              Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
            },
            constraints: table =>
            {
              table.PrimaryKey("PK_Documents", x => x.Id);
            });
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
        migrationBuilder.DropTable(
            name: "Documents");
      }
    }
}
