using Microsoft.EntityFrameworkCore;
using StajyerTakipSistemi.Models;

namespace StajyerTakipSistemi.Data
{
    public class StajyerTakipDbContext : DbContext
    {
        public StajyerTakipDbContext(DbContextOptions<StajyerTakipDbContext> options) : base(options)
        {
        }

        // Mevcut DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Gorev> Gorevler { get; set; }
        public DbSet<StajyerGorev> StajyerGorevler { get; set; }
        public DbSet<Egitim> Egitimler { get; set; }

        // Ödev teslim sistemi için
        public DbSet<OdevTeslim> OdevTeslimler { get; set; }
        public DbSet<OdevMesaj> OdevMesajlar { get; set; }

        // ✅ EKSİK OLAN: EgitimKatilimi DbSet'i
        public DbSet<EgitimKatilimi> EgitimKatilimlari { get; set; }

        public DbSet<SistemBildirimi> SistemBildirimleri { get; set; }

        public DbSet<EgitmenDuyurusu> EgitmenDuyurulari { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mevcut konfigürasyonlar...
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.UserType).HasConversion<int>();
            });

            modelBuilder.Entity<Gorev>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Egitmen)
                    .WithMany()
                    .HasForeignKey(e => e.EgitmenId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StajyerGorev>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.AtananGorevler)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Gorev)
                    .WithMany(g => g.StajyerGorevler)
                    .HasForeignKey(e => e.GorevId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // OdevTeslim konfigürasyonu
            modelBuilder.Entity<OdevTeslim>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.StajyerGorev)
                    .WithMany()
                    .HasForeignKey(e => e.StajyerGorevId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Durum)
                    .HasConversion<int>();
            });

            // OdevMesaj konfigürasyonu
            modelBuilder.Entity<OdevMesaj>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.OdevTeslim)
                    .WithMany(ot => ot.Mesajlar)
                    .HasForeignKey(e => e.OdevTeslimId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Gonderen)
                    .WithMany()
                    .HasForeignKey(e => e.GonderenId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Egitim>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // ✅ EgitimKatilimi konfigürasyonu - ZATEN VAR
            modelBuilder.Entity<EgitimKatilimi>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Egitim)
                    .WithMany(eg => eg.Katilimlar)
                    .HasForeignKey(e => e.EgitimId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Stajyer)
                    .WithMany()
                    .HasForeignKey(e => e.StajyerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // EgitmenDuyurusu konfigürasyonu
            modelBuilder.Entity<EgitmenDuyurusu>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Egitmen)
                    .WithMany()
                    .HasForeignKey(e => e.EgitmenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.HedefKitle)
                    .HasConversion<int>();
            });
        }
    }
}