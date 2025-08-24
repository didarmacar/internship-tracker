using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models.ViewModels
{
    public class YeniEgitimViewModel
    {
        [Required(ErrorMessage = "Eğitim adı gereklidir")]
        [Display(Name = "Eğitim Adı")]
        public string EgitimAdi { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Tarih seçimi gereklidir")]
        [Display(Name = "Eğitim Tarihi")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Saat seçimi gereklidir")]
        [Display(Name = "Eğitim Saati")]
        [DataType(DataType.Time)]
        public TimeSpan Saat { get; set; } = new TimeSpan(10, 0, 0); // 10:00

        [Required(ErrorMessage = "En az bir stajyer seçmelisiniz")]
        public List<int> SeciliStajyerler { get; set; } = new List<int>();
    }
}
