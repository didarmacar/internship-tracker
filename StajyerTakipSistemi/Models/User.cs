using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajyerTakipSistemi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? AdSoyad { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        public string? Sifre { get; set; }

        [Required]
        public UserType UserType { get; set; }

        public DateTime? KayitTarihi { get; set; } = DateTime.Now;

        // Başvuru bilgileri
        public string? BasvuruDurumu { get; set; }
        public DateTime? BasvuruTarihi { get; set; }

        // İletişim bilgileri
        [Phone]
        public string? Telefon { get; set; }

        // Eğitim bilgileri
        [StringLength(200)]
        public string? OkulAdi { get; set; }

        [StringLength(100)]
        public string? Bolum { get; set; }

        [StringLength(10)]
        public string? Sinif { get; set; }

        // Staj bilgileri (sadece stajyerler için)
        [StringLength(50)]
        public string? StajTuru { get; set; }
        public DateTime? StajBaslangicTarihi { get; set; }
        public DateTime? StajBitisTarihi { get; set; }

        // Dosya yolları
        [StringLength(500)]
        public string? CVDosyaYolu { get; set; }

        [StringLength(500)]
        public string? FotografDosyaYolu { get; set; }

        // Navigation property - direkt görev atamaları
        public List<StajyerGorev> AtananGorevler { get; set; } = new List<StajyerGorev>();
    }
}