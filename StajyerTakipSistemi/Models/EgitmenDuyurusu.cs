using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models
{
    public class EgitmenDuyurusu
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Baslik { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Mesaj { get; set; } = string.Empty;

        public int EgitmenId { get; set; }
        public User Egitmen { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public bool Aktif { get; set; } = true;

        // Hangi stajyerlere gönderildi
        public DuyuruHedefKitle HedefKitle { get; set; } = DuyuruHedefKitle.TumStajyerler;
    }

    public enum DuyuruHedefKitle
    {
        TumStajyerler = 0,
        AktifStajyerler = 1,
        BelirliStajyerler = 2
    }
}