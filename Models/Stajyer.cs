using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models
{
    public class Stajyer
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        public string? Telefon { get; set; }

        public string? OkulAdi { get; set; }
        public string? Bolum { get; set; }
        public string? Sinif { get; set; }

        [Required]
        public StajTuru StajTuru { get; set; }

        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }

        public StajDurumu Durum { get; set; } = StajDurumu.OnayBekleyen;

        // Dosyalar
        public string? CVDosyasi { get; set; }
        public string? FotografDosyasi { get; set; }
        public string? SigortaGirisDosyasi { get; set; }
        public string? OgrenciBelgesiDosyasi { get; set; }

        // Navigation property
        public List<StajyerGorev> StajyerGorevler { get; set; } = new List<StajyerGorev>();
    }

    public enum StajTuru
    {
        Zorunlu = 1,
        Gonullu = 2
    }

    public enum StajDurumu
    {
        OnayBekleyen = 1,
        Aktif = 2,
        Reddedilmis = 3,
        Tamamlandi = 4
    }

    public enum StajZamani
    {
        YazDonemi = 1,
        KisDonemi = 2
    }
}