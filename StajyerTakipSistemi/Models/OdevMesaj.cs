using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models
{
    public class OdevMesaj
    {
        public int Id { get; set; }

        public int OdevTeslimId { get; set; }
        public OdevTeslim OdevTeslim { get; set; }

        public int GonderenId { get; set; }
        public User Gonderen { get; set; }

        [Required]
        public string Mesaj { get; set; } = string.Empty;

        public DateTime GonderimTarihi { get; set; } = DateTime.Now;

        public bool Okundu { get; set; } = false;

        public string? EkDosyaYolu { get; set; }
    }
}