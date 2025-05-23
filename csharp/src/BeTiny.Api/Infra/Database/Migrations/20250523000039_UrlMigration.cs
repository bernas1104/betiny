using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeTiny.Api.Infra.Database.Migrations
{
    /// <inheritdoc />
    public partial class UrlMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Urls",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LongUrl = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Accesses = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Urls", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Urls_LongUrl",
                table: "Urls",
                column: "LongUrl",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Urls");
        }
    }
}
