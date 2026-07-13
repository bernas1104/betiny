using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeTiny.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ClickEventIpSizeFixMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "ClickEvents",
                type: "character varying(45)",
                maxLength: 45,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "ClickEvents",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(45)",
                oldMaxLength: 45);
        }
    }
}
