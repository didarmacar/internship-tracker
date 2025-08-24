using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models
{
    public class EgitimKatilimi
    {
        public int Id { get; set; }

        public int EgitimId { get; set; }
        public Egitim Egitim { get; set; }

        public int StajyerId { get; set; }
        public User Stajyer { get; set; }

        public DateTime AtamaTarihi { get; set; } = DateTime.Now;

        public bool? KatildiMi { get; set; } // null=karar vermedi, true=katıldı, false=katılmadı

        public DateTime? KatilimTarihi { get; set; }

        public string? KatilmamaNedeni { get; set; }
    }
}
