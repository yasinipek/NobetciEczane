﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NobetciEczane.Data;

#nullable disable

namespace NobetciEczane.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("NobetciEczane.Models.EczaneModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Adres")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Boylam")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Enlem")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("IlId")
                        .HasColumnType("int");

                    b.Property<string>("Ilce")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Isim")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("KayitZamani")
                        .HasColumnType("datetime2");

                    b.Property<string>("Tarih")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Telefon")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("IlId");

                    b.ToTable("Eczaneler");
                });

            modelBuilder.Entity("NobetciEczane.Models.IlModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("IlAdi")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Iller");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            IlAdi = "ADANA"
                        },
                        new
                        {
                            Id = 2,
                            IlAdi = "ADIYAMAN"
                        },
                        new
                        {
                            Id = 3,
                            IlAdi = "AFYONKARAHİSAR"
                        },
                        new
                        {
                            Id = 4,
                            IlAdi = "AĞRI"
                        },
                        new
                        {
                            Id = 5,
                            IlAdi = "AMASYA"
                        },
                        new
                        {
                            Id = 6,
                            IlAdi = "ANKARA"
                        },
                        new
                        {
                            Id = 7,
                            IlAdi = "ANTALYA"
                        },
                        new
                        {
                            Id = 8,
                            IlAdi = "ARTVİN"
                        },
                        new
                        {
                            Id = 9,
                            IlAdi = "AYDIN"
                        },
                        new
                        {
                            Id = 10,
                            IlAdi = "BALIKESİR"
                        },
                        new
                        {
                            Id = 11,
                            IlAdi = "BİLECİK"
                        },
                        new
                        {
                            Id = 12,
                            IlAdi = "BİNGÖL"
                        },
                        new
                        {
                            Id = 13,
                            IlAdi = "BİTLİS"
                        },
                        new
                        {
                            Id = 14,
                            IlAdi = "BOLU"
                        },
                        new
                        {
                            Id = 15,
                            IlAdi = "BURDUR"
                        },
                        new
                        {
                            Id = 16,
                            IlAdi = "BURSA"
                        },
                        new
                        {
                            Id = 17,
                            IlAdi = "ÇANAKKALE"
                        },
                        new
                        {
                            Id = 18,
                            IlAdi = "ÇANKIRI"
                        },
                        new
                        {
                            Id = 19,
                            IlAdi = "ÇORUM"
                        },
                        new
                        {
                            Id = 20,
                            IlAdi = "DENİZLİ"
                        },
                        new
                        {
                            Id = 21,
                            IlAdi = "DİYARBAKIR"
                        },
                        new
                        {
                            Id = 22,
                            IlAdi = "EDİRNE"
                        },
                        new
                        {
                            Id = 23,
                            IlAdi = "ELAZIĞ"
                        },
                        new
                        {
                            Id = 24,
                            IlAdi = "ERZİNCAN"
                        },
                        new
                        {
                            Id = 25,
                            IlAdi = "ERZURUM"
                        },
                        new
                        {
                            Id = 26,
                            IlAdi = "ESKİŞEHİR"
                        },
                        new
                        {
                            Id = 27,
                            IlAdi = "GAZİANTEP"
                        },
                        new
                        {
                            Id = 28,
                            IlAdi = "GİRESUN"
                        },
                        new
                        {
                            Id = 29,
                            IlAdi = "GÜMÜŞHANE"
                        },
                        new
                        {
                            Id = 30,
                            IlAdi = "HAKKARİ"
                        },
                        new
                        {
                            Id = 31,
                            IlAdi = "HATAY"
                        },
                        new
                        {
                            Id = 32,
                            IlAdi = "ISPARTA"
                        },
                        new
                        {
                            Id = 33,
                            IlAdi = "MERSİN"
                        },
                        new
                        {
                            Id = 34,
                            IlAdi = "İSTANBUL"
                        },
                        new
                        {
                            Id = 35,
                            IlAdi = "İZMİR"
                        },
                        new
                        {
                            Id = 36,
                            IlAdi = "KARS"
                        },
                        new
                        {
                            Id = 37,
                            IlAdi = "KASTAMONU"
                        },
                        new
                        {
                            Id = 38,
                            IlAdi = "KAYSERİ"
                        },
                        new
                        {
                            Id = 39,
                            IlAdi = "KIRKLARELİ"
                        },
                        new
                        {
                            Id = 40,
                            IlAdi = "KIRŞEHİR"
                        },
                        new
                        {
                            Id = 41,
                            IlAdi = "KOCAELİ"
                        },
                        new
                        {
                            Id = 42,
                            IlAdi = "KONYA"
                        },
                        new
                        {
                            Id = 43,
                            IlAdi = "KÜTAHYA"
                        },
                        new
                        {
                            Id = 44,
                            IlAdi = "MALATYA"
                        },
                        new
                        {
                            Id = 45,
                            IlAdi = "MANİSA"
                        },
                        new
                        {
                            Id = 46,
                            IlAdi = "KAHRAMANMARAŞ"
                        },
                        new
                        {
                            Id = 47,
                            IlAdi = "MARDİN"
                        },
                        new
                        {
                            Id = 48,
                            IlAdi = "MUĞLA"
                        },
                        new
                        {
                            Id = 49,
                            IlAdi = "MUŞ"
                        },
                        new
                        {
                            Id = 50,
                            IlAdi = "NEVŞEHİR"
                        },
                        new
                        {
                            Id = 51,
                            IlAdi = "NİĞDE"
                        },
                        new
                        {
                            Id = 52,
                            IlAdi = "ORDU"
                        },
                        new
                        {
                            Id = 53,
                            IlAdi = "RİZE"
                        },
                        new
                        {
                            Id = 54,
                            IlAdi = "SAKARYA"
                        },
                        new
                        {
                            Id = 55,
                            IlAdi = "SAMSUN"
                        },
                        new
                        {
                            Id = 56,
                            IlAdi = "SİİRT"
                        },
                        new
                        {
                            Id = 57,
                            IlAdi = "SİNOP"
                        },
                        new
                        {
                            Id = 58,
                            IlAdi = "SİVAS"
                        },
                        new
                        {
                            Id = 59,
                            IlAdi = "TEKİRDAĞ"
                        },
                        new
                        {
                            Id = 60,
                            IlAdi = "TOKAT"
                        },
                        new
                        {
                            Id = 61,
                            IlAdi = "TRABZON"
                        },
                        new
                        {
                            Id = 62,
                            IlAdi = "TUNCELİ"
                        },
                        new
                        {
                            Id = 63,
                            IlAdi = "ŞANLIURFA"
                        },
                        new
                        {
                            Id = 64,
                            IlAdi = "UŞAK"
                        },
                        new
                        {
                            Id = 65,
                            IlAdi = "VAN"
                        },
                        new
                        {
                            Id = 66,
                            IlAdi = "YOZGAT"
                        },
                        new
                        {
                            Id = 67,
                            IlAdi = "ZONGULDAK"
                        },
                        new
                        {
                            Id = 68,
                            IlAdi = "AKSARAY"
                        },
                        new
                        {
                            Id = 69,
                            IlAdi = "BAYBURT"
                        },
                        new
                        {
                            Id = 70,
                            IlAdi = "KARAMAN"
                        },
                        new
                        {
                            Id = 71,
                            IlAdi = "KIRIKKALE"
                        },
                        new
                        {
                            Id = 72,
                            IlAdi = "BATMAN"
                        },
                        new
                        {
                            Id = 73,
                            IlAdi = "ŞIRNAK"
                        },
                        new
                        {
                            Id = 74,
                            IlAdi = "BARTIN"
                        },
                        new
                        {
                            Id = 75,
                            IlAdi = "ARDAHAN"
                        },
                        new
                        {
                            Id = 76,
                            IlAdi = "IĞDIR"
                        },
                        new
                        {
                            Id = 77,
                            IlAdi = "YALOVA"
                        },
                        new
                        {
                            Id = 78,
                            IlAdi = "KARABÜK"
                        },
                        new
                        {
                            Id = 79,
                            IlAdi = "KİLİS"
                        },
                        new
                        {
                            Id = 80,
                            IlAdi = "OSMANİYE"
                        },
                        new
                        {
                            Id = 81,
                            IlAdi = "DÜZCE"
                        });
                });

            modelBuilder.Entity("NobetciEczane.Models.EczaneModel", b =>
                {
                    b.HasOne("NobetciEczane.Models.IlModel", "Il")
                        .WithMany("Eczaneler")
                        .HasForeignKey("IlId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Il");
                });

            modelBuilder.Entity("NobetciEczane.Models.IlModel", b =>
                {
                    b.Navigation("Eczaneler");
                });
#pragma warning restore 612, 618
        }
    }
}
