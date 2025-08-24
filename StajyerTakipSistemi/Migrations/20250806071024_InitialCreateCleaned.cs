using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateCleaned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Egitimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EgitimAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Saat = table.Column<TimeSpan>(type: "time", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Egitimler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BasvuruDurumu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BasvuruTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OkulAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Bolum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Sinif = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    StajTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StajBaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StajBitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CVDosyaYolu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FotografDosyaYolu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gorevler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GorevAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EgitmenId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gorevler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gorevler_Users_EgitmenId",
                        column: x => x.EgitmenId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stajyerler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OkulAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bolum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sinif = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StajTuru = table.Column<int>(type: "int", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    CVDosyasi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FotografDosyasi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SigortaGirisDosyasi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OgrenciBelgesiDosyasi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stajyerler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stajyerler_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjeAdimlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StajyerId = table.Column<int>(type: "int", nullable: false),
                    Baslik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tamamlandi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeAdimlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjeAdimlari_Stajyerler_StajyerId",
                        column: x => x.StajyerId,
                        principalTable: "Stajyerler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StajyerGorevler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StajyerId = table.Column<int>(type: "int", nullable: false),
                    GorevId = table.Column<int>(type: "int", nullable: false),
                    AtamaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tamamlandi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StajyerGorevler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StajyerGorevler_Gorevler_GorevId",
                        column: x => x.GorevId,
                        principalTable: "Gorevler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StajyerGorevler_Stajyerler_StajyerId",
                        column: x => x.StajyerId,
                        principalTable: "Stajyerler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_EgitmenId",
                table: "Gorevler",
                column: "EgitmenId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeAdimlari_StajyerId",
                table: "ProjeAdimlari",
                column: "StajyerId");

            migrationBuilder.CreateIndex(
                name: "IX_StajyerGorevler_GorevId",
                table: "StajyerGorevler",
                column: "GorevId");

            migrationBuilder.CreateIndex(
                name: "IX_StajyerGorevler_StajyerId",
                table: "StajyerGorevler",
                column: "StajyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Stajyerler_UserId",
                table: "Stajyerler",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Egitimler");

            migrationBuilder.DropTable(
                name: "ProjeAdimlari");

            migrationBuilder.DropTable(
                name: "StajyerGorevler");

            migrationBuilder.DropTable(
                name: "Gorevler");

            migrationBuilder.DropTable(
                name: "Stajyerler");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
