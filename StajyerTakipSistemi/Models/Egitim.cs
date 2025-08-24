using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models
{
    public class Egitim
    {
        public int Id { get; set; }

        [Required]
        public string EgitimAdi { get; set; }

        public DateTime Tarih { get; set; }
        public TimeSpan Saat { get; set; }

        public string? Aciklama { get; set; }

        // ❌ HATALI - Model içinde DbSet olmamalı
        // public DbSet<EgitimKatilimi> EgitimKatilimlari { get; set; }

        // ✅ DOĞRU - Navigation property
        public List<EgitimKatilimi> Katilimlar { get; set; } = new List<EgitimKatilimi>();
    }
}