using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models.ViewModels
{
    public class YeniOdevViewModel
    {
        [Required(ErrorMessage = "Görev adı gereklidir")]
        public string GorevAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama gereklidir")]
        public string Aciklama { get; set; } = string.Empty;

        [Required(ErrorMessage = "Teslim tarihi gereklidir")]
        public DateTime TeslimTarihi { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "Zorluk seviyesi gereklidir")]
        public string ZorlukSeviyesi { get; set; } = string.Empty;

        public List<int> SeciliStajyerler { get; set; } = new List<int>();
    }
}