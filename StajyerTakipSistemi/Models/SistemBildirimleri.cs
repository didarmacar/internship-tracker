using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models
{
    public class SistemBildirimi
    {
        public int Id { get; set; }

        public int StajyerId { get; set; }
        public User Stajyer { get; set; }

        [Required]
        public string Baslik { get; set; } = string.Empty;

        [Required]
        public string Mesaj { get; set; } = string.Empty;

        public BildirimTuru Tur { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public bool Okundu { get; set; } = false;

        // İlgili kayıt ID'leri (ödev/eğitim)
        public int? IlgiliId { get; set; }
    }

    public enum BildirimTuru
    {
        OdevAtamasi = 0,
        EgitimAtamasi = 1,
        EgitmenDuyurusu = 2
    }
}