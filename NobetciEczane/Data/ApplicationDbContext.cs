using Microsoft.EntityFrameworkCore;
using NobetciEczane.Models;

namespace NobetciEczane.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<IlModel> Iller { get; set; }
        public DbSet<EczaneModel> Eczaneler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IlModel>().HasData(
                new IlModel { Id = 1, IlAdi = "ADANA" },
                new IlModel { Id = 2, IlAdi = "ADIYAMAN" },
                new IlModel { Id = 3, IlAdi = "AFYONKARAHİSAR" },
                new IlModel { Id = 4, IlAdi = "AĞRI" },
                new IlModel { Id = 5, IlAdi = "AMASYA" },
                new IlModel { Id = 6, IlAdi = "ANKARA" },
                new IlModel { Id = 7, IlAdi = "ANTALYA" },
                new IlModel { Id = 8, IlAdi = "ARTVİN" },
                new IlModel { Id = 9, IlAdi = "AYDIN" },
                new IlModel { Id = 10, IlAdi = "BALIKESİR" },
                new IlModel { Id = 11, IlAdi = "BİLECİK" },
                new IlModel { Id = 12, IlAdi = "BİNGÖL" },
                new IlModel { Id = 13, IlAdi = "BİTLİS" },
                new IlModel { Id = 14, IlAdi = "BOLU" },
                new IlModel { Id = 15, IlAdi = "BURDUR" },
                new IlModel { Id = 16, IlAdi = "BURSA" },
                new IlModel { Id = 17, IlAdi = "ÇANAKKALE" },
                new IlModel { Id = 18, IlAdi = "ÇANKIRI" },
                new IlModel { Id = 19, IlAdi = "ÇORUM" },
                new IlModel { Id = 20, IlAdi = "DENİZLİ" },
                new IlModel { Id = 21, IlAdi = "DİYARBAKIR" },
                new IlModel { Id = 22, IlAdi = "EDİRNE" },
                new IlModel { Id = 23, IlAdi = "ELAZIĞ" },
                new IlModel { Id = 24, IlAdi = "ERZİNCAN" },
                new IlModel { Id = 25, IlAdi = "ERZURUM" },
                new IlModel { Id = 26, IlAdi = "ESKİŞEHİR" },
                new IlModel { Id = 27, IlAdi = "GAZİANTEP" },
                new IlModel { Id = 28, IlAdi = "GİRESUN" },
                new IlModel { Id = 29, IlAdi = "GÜMÜŞHANE" },
                new IlModel { Id = 30, IlAdi = "HAKKARİ" },
                new IlModel { Id = 31, IlAdi = "HATAY" },
                new IlModel { Id = 32, IlAdi = "ISPARTA" },
                new IlModel { Id = 33, IlAdi = "MERSİN" },
                new IlModel { Id = 34, IlAdi = "İSTANBUL" },
                new IlModel { Id = 35, IlAdi = "İZMİR" },
                new IlModel { Id = 36, IlAdi = "KARS" },
                new IlModel { Id = 37, IlAdi = "KASTAMONU" },
                new IlModel { Id = 38, IlAdi = "KAYSERİ" },
                new IlModel { Id = 39, IlAdi = "KIRKLARELİ" },
                new IlModel { Id = 40, IlAdi = "KIRŞEHİR" },
                new IlModel { Id = 41, IlAdi = "KOCAELİ" },
                new IlModel { Id = 42, IlAdi = "KONYA" },
                new IlModel { Id = 43, IlAdi = "KÜTAHYA" },
                new IlModel { Id = 44, IlAdi = "MALATYA" },
                new IlModel { Id = 45, IlAdi = "MANİSA" },
                new IlModel { Id = 46, IlAdi = "KAHRAMANMARAŞ" },
                new IlModel { Id = 47, IlAdi = "MARDİN" },
                new IlModel { Id = 48, IlAdi = "MUĞLA" },
                new IlModel { Id = 49, IlAdi = "MUŞ" },
                new IlModel { Id = 50, IlAdi = "NEVŞEHİR" },
                new IlModel { Id = 51, IlAdi = "NİĞDE" },
                new IlModel { Id = 52, IlAdi = "ORDU" },
                new IlModel { Id = 53, IlAdi = "RİZE" },
                new IlModel { Id = 54, IlAdi = "SAKARYA" },
                new IlModel { Id = 55, IlAdi = "SAMSUN" },
                new IlModel { Id = 56, IlAdi = "SİİRT" },
                new IlModel { Id = 57, IlAdi = "SİNOP" },
                new IlModel { Id = 58, IlAdi = "SİVAS" },
                new IlModel { Id = 59, IlAdi = "TEKİRDAĞ" },
                new IlModel { Id = 60, IlAdi = "TOKAT" },
                new IlModel { Id = 61, IlAdi = "TRABZON" },
                new IlModel { Id = 62, IlAdi = "TUNCELİ" },
                new IlModel { Id = 63, IlAdi = "ŞANLIURFA" },
                new IlModel { Id = 64, IlAdi = "UŞAK" },
                new IlModel { Id = 65, IlAdi = "VAN" },
                new IlModel { Id = 66, IlAdi = "YOZGAT" },
                new IlModel { Id = 67, IlAdi = "ZONGULDAK" },
                new IlModel { Id = 68, IlAdi = "AKSARAY" },
                new IlModel { Id = 69, IlAdi = "BAYBURT" },
                new IlModel { Id = 70, IlAdi = "KARAMAN" },
                new IlModel { Id = 71, IlAdi = "KIRIKKALE" },
                new IlModel { Id = 72, IlAdi = "BATMAN" },
                new IlModel { Id = 73, IlAdi = "ŞIRNAK" },
                new IlModel { Id = 74, IlAdi = "BARTIN" },
                new IlModel { Id = 75, IlAdi = "ARDAHAN" },
                new IlModel { Id = 76, IlAdi = "IĞDIR" },
                new IlModel { Id = 77, IlAdi = "YALOVA" },
                new IlModel { Id = 78, IlAdi = "KARABÜK" },
                new IlModel { Id = 79, IlAdi = "KİLİS" },
                new IlModel { Id = 80, IlAdi = "OSMANİYE" },
                new IlModel { Id = 81, IlAdi = "DÜZCE" }
            );
        }
    }
}
