namespace StajyerTakipSistemi.Models.ViewModels
{
    public class StajyerDashboardViewModel
    {
        public string KullaniciAdi { get; set; } = string.Empty;
        public string KullaniciInitials { get; set; } = string.Empty;
        public List<OdevViewModel> BekleyenOdevler { get; set; } = new List<OdevViewModel>();
        public List<EgitimViewModel> BugunEgitimleri { get; set; } = new List<EgitimViewModel>();
        public int GenelIlerleme { get; set; }
        public int BuHaftaIlerleme { get; set; }
        public List<AktiviteViewModel> SonAktiviteler { get; set; } = new List<AktiviteViewModel>();
    }

    public class OdevViewModel
    {
        public string GorevAdi { get; set; } = string.Empty;
        public string Durum { get; set; } = string.Empty;
    }

    public class EgitimViewModel
    {
        public string Saat { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
    }

    public class AktiviteViewModel
    {
        public string Icon { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
    }
}