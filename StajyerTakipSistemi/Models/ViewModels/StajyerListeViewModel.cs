namespace StajyerTakipSistemi.Models.ViewModels
{
    public class StajyerListeViewModel
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string OkulAdi { get; set; } = string.Empty;
        public string Bolum { get; set; } = string.Empty;
        public string StajTuru { get; set; } = string.Empty;
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public string Durum { get; set; } = string.Empty;
        public int IlerlemeYuzdesi { get; set; }
        public string IlerlemeMetni { get; set; } = string.Empty;
    }
}