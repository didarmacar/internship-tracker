using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models.ViewModels
{
    public class OdevTeslimViewModel
    {
        public int StajyerGorevId { get; set; }

        [Required(ErrorMessage = "Lütfen bir dosya seçiniz")]
        public IFormFile OdevDosyasi { get; set; }

        [StringLength(500, ErrorMessage = "Not 500 karakterden uzun olamaz")]
        public string? StajyerNotu { get; set; }
    }

    public class OdevDegerlendirmeViewModel
    {
        public int TeslimId { get; set; }
        public string StajyerAdi { get; set; } = string.Empty;
        public string GorevAdi { get; set; } = string.Empty;
        public string DosyaAdi { get; set; } = string.Empty;
        public string DosyaYolu { get; set; } = string.Empty;
        public DateTime TeslimTarihi { get; set; }
        public string? StajyerNotu { get; set; }

        [Range(0, 100, ErrorMessage = "Puan 0-100 arasında olmalıdır")]
        public int? Puan { get; set; }

        [StringLength(1000, ErrorMessage = "Geri bildirim 1000 karakterden uzun olamaz")]
        public string? EgitmenGeriBildirimi { get; set; }

        public TeslimDurumu Durum { get; set; }
    }

    public class YeniMesajViewModel
    {
        public int OdevTeslimId { get; set; }

        [Required(ErrorMessage = "Mesaj boş olamaz")]
        [StringLength(1000, ErrorMessage = "Mesaj 1000 karakterden uzun olamaz")]
        public string Mesaj { get; set; } = string.Empty;

        public IFormFile? EkDosya { get; set; }
    }
}