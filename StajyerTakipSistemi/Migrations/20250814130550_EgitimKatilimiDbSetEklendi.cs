using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class EgitimKatilimiDbSetEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EgitimKatilimi_Egitimler_EgitimId",
                table: "EgitimKatilimi");

            migrationBuilder.DropForeignKey(
                name: "FK_EgitimKatilimi_Egitimler_EgitimId1",
                table: "EgitimKatilimi");

            migrationBuilder.DropForeignKey(
                name: "FK_EgitimKatilimi_Users_StajyerId",
                table: "EgitimKatilimi");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EgitimKatilimi",
                table: "EgitimKatilimi");

            migrationBuilder.DropIndex(
                name: "IX_EgitimKatilimi_EgitimId1",
                table: "EgitimKatilimi");

            migrationBuilder.DropColumn(
                name: "EgitimId1",
                table: "EgitimKatilimi");

            migrationBuilder.RenameTable(
                name: "EgitimKatilimi",
                newName: "EgitimKatilimlari");

            migrationBuilder.RenameIndex(
                name: "IX_EgitimKatilimi_StajyerId",
                table: "EgitimKatilimlari",
                newName: "IX_EgitimKatilimlari_StajyerId");

            migrationBuilder.RenameIndex(
                name: "IX_EgitimKatilimi_EgitimId",
                table: "EgitimKatilimlari",
                newName: "IX_EgitimKatilimlari_EgitimId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EgitimKatilimlari",
                table: "EgitimKatilimlari",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EgitimKatilimlari_Egitimler_EgitimId",
                table: "EgitimKatilimlari",
                column: "EgitimId",
                principalTable: "Egitimler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EgitimKatilimlari_Users_StajyerId",
                table: "EgitimKatilimlari",
                column: "StajyerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EgitimKatilimlari_Egitimler_EgitimId",
                table: "EgitimKatilimlari");

            migrationBuilder.DropForeignKey(
                name: "FK_EgitimKatilimlari_Users_StajyerId",
                table: "EgitimKatilimlari");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EgitimKatilimlari",
                table: "EgitimKatilimlari");

            migrationBuilder.RenameTable(
                name: "EgitimKatilimlari",
                newName: "EgitimKatilimi");

            migrationBuilder.RenameIndex(
                name: "IX_EgitimKatilimlari_StajyerId",
                table: "EgitimKatilimi",
                newName: "IX_EgitimKatilimi_StajyerId");

            migrationBuilder.RenameIndex(
                name: "IX_EgitimKatilimlari_EgitimId",
                table: "EgitimKatilimi",
                newName: "IX_EgitimKatilimi_EgitimId");

            migrationBuilder.AddColumn<int>(
                name: "EgitimId1",
                table: "EgitimKatilimi",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EgitimKatilimi",
                table: "EgitimKatilimi",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EgitimKatilimi_EgitimId1",
                table: "EgitimKatilimi",
                column: "EgitimId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EgitimKatilimi_Egitimler_EgitimId",
                table: "EgitimKatilimi",
                column: "EgitimId",
                principalTable: "Egitimler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EgitimKatilimi_Egitimler_EgitimId1",
                table: "EgitimKatilimi",
                column: "EgitimId1",
                principalTable: "Egitimler",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EgitimKatilimi_Users_StajyerId",
                table: "EgitimKatilimi",
                column: "StajyerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
