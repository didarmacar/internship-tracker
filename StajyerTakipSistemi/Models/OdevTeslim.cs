using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models
{
    public class OdevTeslim
    {
        public int Id { get; set; }

        public int StajyerGorevId { get; set; }
        public StajyerGorev StajyerGorev { get; set; }

        [Required]
        public string DosyaYolu { get; set; } = string.Empty;

        [Required]
        public string DosyaAdi { get; set; } = string.Empty;

        public long DosyaBoyutu { get; set; }

        public DateTime TeslimTarihi { get; set; } = DateTime.Now;

        public string? StajyerNotu { get; set; }

        public TeslimDurumu Durum { get; set; } = TeslimDurumu.Beklemede;

        public DateTime? DegerlendirmeTarihi { get; set; }

        public string? EgitmenGeriBildirimi { get; set; }

        public int? Puan { get; set; }

        public List<OdevMesaj> Mesajlar { get; set; } = new List<OdevMesaj>();
    }

    public enum TeslimDurumu
    {
        Beklemede = 0,
        Degerlendiriliyor = 1,
        Onaylandi = 2,
        Reddedildi = 3,
        RevizeDeneme = 4
    }
}