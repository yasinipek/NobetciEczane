using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NobetciEczane.Migrations
{
    /// <inheritdoc />
    public partial class mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Iller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IlAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Iller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Eczaneler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Isim = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IlId = table.Column<int>(type: "int", nullable: false),
                    Ilce = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Enlem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Boylam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KayitZamani = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eczaneler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Eczaneler_Iller_IlId",
                        column: x => x.IlId,
                        principalTable: "Iller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Iller",
                columns: new[] { "Id", "IlAdi" },
                values: new object[,]
                {
                    { 1, "ADANA" },
                    { 2, "ADIYAMAN" },
                    { 3, "AFYONKARAHİSAR" },
                    { 4, "AĞRI" },
                    { 5, "AMASYA" },
                    { 6, "ANKARA" },
                    { 7, "ANTALYA" },
                    { 8, "ARTVİN" },
                    { 9, "AYDIN" },
                    { 10, "BALIKESİR" },
                    { 11, "BİLECİK" },
                    { 12, "BİNGÖL" },
                    { 13, "BİTLİS" },
                    { 14, "BOLU" },
                    { 15, "BURDUR" },
                    { 16, "BURSA" },
                    { 17, "ÇANAKKALE" },
                    { 18, "ÇANKIRI" },
                    { 19, "ÇORUM" },
                    { 20, "DENİZLİ" },
                    { 21, "DİYARBAKIR" },
                    { 22, "EDİRNE" },
                    { 23, "ELAZIĞ" },
                    { 24, "ERZİNCAN" },
                    { 25, "ERZURUM" },
                    { 26, "ESKİŞEHİR" },
                    { 27, "GAZİANTEP" },
                    { 28, "GİRESUN" },
                    { 29, "GÜMÜŞHANE" },
                    { 30, "HAKKARİ" },
                    { 31, "HATAY" },
                    { 32, "ISPARTA" },
                    { 33, "MERSİN" },
                    { 34, "İSTANBUL" },
                    { 35, "İZMİR" },
                    { 36, "KARS" },
                    { 37, "KASTAMONU" },
                    { 38, "KAYSERİ" },
                    { 39, "KIRKLARELİ" },
                    { 40, "KIRŞEHİR" },
                    { 41, "KOCAELİ" },
                    { 42, "KONYA" },
                    { 43, "KÜTAHYA" },
                    { 44, "MALATYA" },
                    { 45, "MANİSA" },
                    { 46, "KAHRAMANMARAŞ" },
                    { 47, "MARDİN" },
                    { 48, "MUĞLA" },
                    { 49, "MUŞ" },
                    { 50, "NEVŞEHİR" },
                    { 51, "NİĞDE" },
                    { 52, "ORDU" },
                    { 53, "RİZE" },
                    { 54, "SAKARYA" },
                    { 55, "SAMSUN" },
                    { 56, "SİİRT" },
                    { 57, "SİNOP" },
                    { 58, "SİVAS" },
                    { 59, "TEKİRDAĞ" },
                    { 60, "TOKAT" },
                    { 61, "TRABZON" },
                    { 62, "TUNCELİ" },
                    { 63, "ŞANLIURFA" },
                    { 64, "UŞAK" },
                    { 65, "VAN" },
                    { 66, "YOZGAT" },
                    { 67, "ZONGULDAK" },
                    { 68, "AKSARAY" },
                    { 69, "BAYBURT" },
                    { 70, "KARAMAN" },
                    { 71, "KIRIKKALE" },
                    { 72, "BATMAN" },
                    { 73, "ŞIRNAK" },
                    { 74, "BARTIN" },
                    { 75, "ARDAHAN" },
                    { 76, "IĞDIR" },
                    { 77, "YALOVA" },
                    { 78, "KARABÜK" },
                    { 79, "KİLİS" },
                    { 80, "OSMANİYE" },
                    { 81, "DÜZCE" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Eczaneler_IlId",
                table: "Eczaneler",
                column: "IlId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Eczaneler");

            migrationBuilder.DropTable(
                name: "Iller");
        }
    }
}
