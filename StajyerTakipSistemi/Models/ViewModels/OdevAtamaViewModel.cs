using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models.ViewModels
{
    public class OdevAtamaViewModel
    {
        public int Id { get; set; }
        public string OdevAdi { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public DateTime OlusturmaTarihi { get; set; }
        public DateTime TeslimTarihi { get; set; }
        public string ZorlukSeviyesi { get; set; } = string.Empty; // Kolay, Orta, Zor
        public List<int> AtananStajyerler { get; set; } = new List<int>();
        public List<string> AtananStajyerAdlari { get; set; } = new List<string>();
        public int TamamlananSayi { get; set; }
        public int ToplamAtanan { get; set; }
        public string Durum { get; set; } = string.Empty; // Aktif, Tamamlandı, Süresi Doldu
    }

    public class YeniOdevViewModel
    {
        [Required(ErrorMessage = "Ödev adı gereklidir")]
        public string OdevAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama gereklidir")]
        public string Aciklama { get; set; } = string.Empty;

        [Required(ErrorMessage = "Teslim tarihi gereklidir")]
        public DateTime TeslimTarihi { get; set; }

        [Required(ErrorMessage = "Zorluk seviyesi seçmelisiniz")]
        public string ZorlukSeviyesi { get; set; } = string.Empty;

        public List<int> SeciliStajyerler { get; set; } = new List<int>();
    }
}