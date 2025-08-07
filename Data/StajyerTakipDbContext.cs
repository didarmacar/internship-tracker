using Microsoft.EntityFrameworkCore;
using StajyerTakipSistemi.Models;

namespace StajyerTakipSistemi.Data
{
    public class StajyerTakipDbContext : DbContext
    {
        public StajyerTakipDbContext(DbContextOptions<StajyerTakipDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Stajyer> Stajyerler { get; set; }
        public DbSet<Gorev> Gorevler { get; set; }
        public DbSet<StajyerGorev> StajyerGorevler { get; set; }
        public DbSet<ProjeAdimi> ProjeAdimlari { get; set; }
        public DbSet<Egitim> Egitimler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.UserType)
                    .HasConversion<int>();
            });

            // Stajyer entity configuration
            modelBuilder.Entity<Stajyer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.StajTuru)
                    .HasConversion<int>();

                entity.Property(e => e.Durum)
                    .HasConversion<int>();
            });

            // Gorev entity configuration
            modelBuilder.Entity<Gorev>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Egitmen)
                    .WithMany()
                    .HasForeignKey(e => e.EgitmenId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // StajyerGorev many-to-many relationship
            modelBuilder.Entity<StajyerGorev>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Stajyer)
                    .WithMany(s => s.StajyerGorevler)
                    .HasForeignKey(e => e.StajyerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Gorev)
                    .WithMany(g => g.StajyerGorevler)
                    .HasForeignKey(e => e.GorevId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ProjeAdimi entity configuration
            modelBuilder.Entity<ProjeAdimi>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Stajyer)
                    .WithMany()
                    .HasForeignKey(e => e.StajyerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Egitim entity configuration
            modelBuilder.Entity<Egitim>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}