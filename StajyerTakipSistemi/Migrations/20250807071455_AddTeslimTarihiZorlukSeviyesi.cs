using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddTeslimTarihiZorlukSeviyesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TeslimTarihi",
                table: "Gorevler",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ZorlukSeviyesi",
                table: "Gorevler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeslimTarihi",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "ZorlukSeviyesi",
                table: "Gorevler");
        }
    }
}
