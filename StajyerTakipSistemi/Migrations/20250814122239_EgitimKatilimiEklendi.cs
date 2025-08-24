using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class EgitimKatilimiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EgitimKatilimi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EgitimId = table.Column<int>(type: "int", nullable: false),
                    StajyerId = table.Column<int>(type: "int", nullable: false),
                    AtamaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KatildiMi = table.Column<bool>(type: "bit", nullable: true),
                    KatilimTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KatilmamaNedeni = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EgitimId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EgitimKatilimi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EgitimKatilimi_Egitimler_EgitimId",
                        column: x => x.EgitimId,
                        principalTable: "Egitimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EgitimKatilimi_Egitimler_EgitimId1",
                        column: x => x.EgitimId1,
                        principalTable: "Egitimler",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EgitimKatilimi_Users_StajyerId",
                        column: x => x.StajyerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EgitimKatilimi_EgitimId",
                table: "EgitimKatilimi",
                column: "EgitimId");

            migrationBuilder.CreateIndex(
                name: "IX_EgitimKatilimi_EgitimId1",
                table: "EgitimKatilimi",
                column: "EgitimId1");

            migrationBuilder.CreateIndex(
                name: "IX_EgitimKatilimi_StajyerId",
                table: "EgitimKatilimi",
                column: "StajyerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EgitimKatilimi");
        }
    }
}
