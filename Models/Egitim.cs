using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models
{
    public class Egitim
    {
        public int Id { get; set; }

        [Required]
        public string EgitimAdi { get; set; }

        public DateTime Tarih { get; set; }
        public TimeSpan Saat { get; set; }

        public string? Aciklama { get; set; }
    }
}