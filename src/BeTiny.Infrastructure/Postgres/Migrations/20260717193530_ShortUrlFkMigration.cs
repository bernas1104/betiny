using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeTiny.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ShortUrlFkMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_UserId",
                table: "ShortUrls",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShortUrls_Users_UserId",
                table: "ShortUrls",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShortUrls_Users_UserId",
                table: "ShortUrls");

            migrationBuilder.DropIndex(
                name: "IX_ShortUrls_UserId",
                table: "ShortUrls");
        }
    }
}
