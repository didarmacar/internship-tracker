using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models.ViewModels
{
    public class StajyerOdevViewModel
    {
        public int StajyerGorevId { get; set; }
        public int GorevId { get; set; }
        public string GorevAdi { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public DateTime AtamaTarihi { get; set; }
        public DateTime TeslimTarihi { get; set; }
        public string ZorlukSeviyesi { get; set; } = string.Empty;
        public bool Tamamlandi { get; set; }

        // Teslim bilgileri
        public bool TeslimEdildi { get; set; }
        public OdevTeslimDetayViewModel? TeslimDetay { get; set; }

        // Durum bilgileri
        public string Durum { get; set; } = string.Empty; // "Bekliyor", "Teslim Edildi", "Onaylandı", vb.
        public string DurumClass { get; set; } = string.Empty; // CSS class
        public bool GeciktiMi { get; set; }
        public int KalanGun { get; set; }
    }

    public class OdevTeslimDetayViewModel
    {
        public int Id { get; set; }
        public string DosyaAdi { get; set; } = string.Empty;
        public string DosyaYolu { get; set; } = string.Empty;
        public long DosyaBoyutu { get; set; }
        public DateTime TeslimTarihi { get; set; }
        public string? StajyerNotu { get; set; }
        public TeslimDurumu Durum { get; set; }
        public DateTime? DegerlendirmeTarihi { get; set; }
        public string? EgitmenGeriBildirimi { get; set; }
        public int? Puan { get; set; }
        public List<OdevMesajViewModel> Mesajlar { get; set; } = new List<OdevMesajViewModel>();
    }

    public class OdevMesajViewModel
    {
        public int Id { get; set; }
        public string GonderenAdi { get; set; } = string.Empty;
        public string Mesaj { get; set; } = string.Empty;
        public DateTime GonderimTarihi { get; set; }
        public bool BenimMesajim { get; set; }
        public string? EkDosyaYolu { get; set; }
    }
}