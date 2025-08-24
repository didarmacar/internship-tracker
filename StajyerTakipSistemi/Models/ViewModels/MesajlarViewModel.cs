namespace StajyerTakipSistemi.Models.ViewModels
{
    public class MesajlarViewModel
    {
        public List<SistemBildirimiViewModel> SistemBildirimleri { get; set; } = new List<SistemBildirimiViewModel>();
        public List<EgitmenDuyuruViewModel> EgitmenDuyurulari { get; set; } = new List<EgitmenDuyuruViewModel>();
    }

    public class SistemBildirimiViewModel
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string Mesaj { get; set; } = string.Empty;
        public DateTime OlusturmaTarihi { get; set; }
        public bool Okundu { get; set; }
        public BildirimTuru Tur { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string ZamanGosterimi { get; set; } = string.Empty;
    }

    public class EgitmenDuyuruViewModel
    {
        public string Baslik { get; set; } = string.Empty;
        public string Mesaj { get; set; } = string.Empty;
        public DateTime Tarih { get; set; }
        public string ZamanGosterimi { get; set; } = string.Empty;
    }
}