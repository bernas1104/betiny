using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeTiny.Infrastructure.Postgres.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class ShortUrlTypeMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShortUrls_ShortCode",
                table: "ShortUrls");

            migrationBuilder.DropColumn(
                name: "ShortCode",
                table: "ShortUrls");

            migrationBuilder.RenameColumn(
                name: "CustomAlias",
                table: "ShortUrls",
                newName: "AliasUrl");

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_AliasUrl",
                table: "ShortUrls",
                column: "AliasUrl",
                unique: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ShortUrls",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "ShortUrls");

            migrationBuilder.RenameColumn(
                name: "AliasUrl",
                table: "ShortUrls",
                newName: "CustomAlias");

            migrationBuilder.DropIndex(
                name: "IX_ShortUrls_AliasUrl",
                table: "ShortUrls");

            migrationBuilder.AddColumn<string>(
                name: "ShortCode",
                table: "ShortUrls",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_ShortCode",
                table: "ShortUrls",
                column: "ShortCode",
                unique: true);
        }
    }
}
