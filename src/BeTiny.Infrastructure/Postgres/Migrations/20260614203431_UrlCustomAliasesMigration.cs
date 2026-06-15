using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeTiny.Infrastructure.Postgres.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class UrlCustomAliasesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShortCode",
                table: "ShortUrls",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7);

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_CustomAlias",
                table: "ShortUrls",
                column: "CustomAlias",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShortUrls_CustomAlias",
                table: "ShortUrls");

            migrationBuilder.AlterColumn<string>(
                name: "ShortCode",
                table: "ShortUrls",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7,
                oldNullable: true);
        }
    }
}
