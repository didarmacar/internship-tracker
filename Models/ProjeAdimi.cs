using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models
{
    public class ProjeAdimi
    {
        public int Id { get; set; }

        public int StajyerId { get; set; }
        public Stajyer Stajyer { get; set; }

        [Required]
        public string Baslik { get; set; }

        public string? Aciklama { get; set; }

        public DateTime Tarih { get; set; }

        public bool Tamamlandi { get; set; } = false;
    }
}
