using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models
{
    public class Egitim
    {
        public int Id { get; set; }

        [Required]
        public string EgitimAdi { get; set; }
        
        [Required]
        public string Baslik { get; set; }

        public DateTime Tarih { get; set; }
        public TimeSpan Saat { get; set; }
        public TimeSpan BaslangicSaati { get; set; }
        public TimeSpan BitisSaati { get; set; }

        public string? Aciklama { get; set; }
    }
}