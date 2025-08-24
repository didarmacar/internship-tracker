namespace StajyerTakipSistemi.Models.ViewModels
{
    public class StajyerDetayViewModel
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string OkulAdi { get; set; } = string.Empty;
        public string Bolum { get; set; } = string.Empty;
        public string Sinif { get; set; } = string.Empty;
        public string StajTuru { get; set; } = string.Empty;
        public DateTime StajBaslangicTarihi { get; set; }
        public DateTime StajBitisTarihi { get; set; }
        public DateTime BasvuruTarihi { get; set; }
        public string? CVDosyaYolu { get; set; }
        public string? FotografDosyaYolu { get; set; }
        public string BasvuruDurumu { get; set; } = string.Empty;
        public int IlerlemeYuzdesi { get; set; }
    }
}