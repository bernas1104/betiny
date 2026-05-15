using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeTiny.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class BasicIndexesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_ShortCode",
                table: "ShortUrls",
                column: "ShortCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClickEvents_CreatedAt",
                table: "ClickEvents",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ClickEvents_ShortUrlId",
                table: "ClickEvents",
                column: "ShortUrlId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClickEvents_ShortUrls_ShortUrlId",
                table: "ClickEvents",
                column: "ShortUrlId",
                principalTable: "ShortUrls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClickEvents_ShortUrls_ShortUrlId",
                table: "ClickEvents");

            migrationBuilder.DropIndex(
                name: "IX_ShortUrls_ShortCode",
                table: "ShortUrls");

            migrationBuilder.DropIndex(
                name: "IX_ClickEvents_CreatedAt",
                table: "ClickEvents");

            migrationBuilder.DropIndex(
                name: "IX_ClickEvents_ShortUrlId",
                table: "ClickEvents");
        }
    }
}
