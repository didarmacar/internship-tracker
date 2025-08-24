using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddOdevTeslimTablelari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sadece yeni tabloları ekle
            migrationBuilder.CreateTable(
                name: "OdevTeslimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StajyerGorevId = table.Column<int>(type: "int", nullable: false),
                    DosyaYolu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DosyaAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DosyaBoyutu = table.Column<long>(type: "bigint", nullable: false),
                    TeslimTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StajyerNotu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    DegerlendirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EgitmenGeriBildirimi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Puan = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdevTeslimler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OdevTeslimler_StajyerGorevler_StajyerGorevId",
                        column: x => x.StajyerGorevId,
                        principalTable: "StajyerGorevler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OdevMesajlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OdevTeslimId = table.Column<int>(type: "int", nullable: false),
                    GonderenId = table.Column<int>(type: "int", nullable: false),
                    Mesaj = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GonderimTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Okundu = table.Column<bool>(type: "bit", nullable: false),
                    EkDosyaYolu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdevMesajlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OdevMesajlar_OdevTeslimler_OdevTeslimId",
                        column: x => x.OdevTeslimId,
                        principalTable: "OdevTeslimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OdevMesajlar_Users_GonderenId",
                        column: x => x.GonderenId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OdevMesajlar_GonderenId",
                table: "OdevMesajlar",
                column: "GonderenId");

            migrationBuilder.CreateIndex(
                name: "IX_OdevMesajlar_OdevTeslimId",
                table: "OdevMesajlar",
                column: "OdevTeslimId");

            migrationBuilder.CreateIndex(
                name: "IX_OdevTeslimler_StajyerGorevId",
                table: "OdevTeslimler",
                column: "StajyerGorevId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OdevMesajlar");

            migrationBuilder.DropTable(
                name: "OdevTeslimler");
        }
    }
}