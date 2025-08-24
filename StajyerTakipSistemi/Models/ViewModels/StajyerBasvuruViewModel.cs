using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models.ViewModels
{
    public class StajyerBasvuruViewModel
    {
        [Required(ErrorMessage = "Ad Soyad gereklidir")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-mail adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        public string Telefon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Staj türü seçimi gereklidir")]
        public string StajTuru { get; set; } = string.Empty;

        [Required(ErrorMessage = "Staj başlangıç tarihi gereklidir")]
        [DataType(DataType.Date)]
        public DateTime StajBaslangicTarihi { get; set; }

        [Required(ErrorMessage = "Staj bitiş tarihi gereklidir")]
        [DataType(DataType.Date)]
        public DateTime StajBitisTarihi { get; set; }

        [Required(ErrorMessage = "Okul adı gereklidir")]
        public string OkulAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bölüm bilgisi gereklidir")]
        public string Bolum { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sınıf bilgisi gereklidir")]
        public string? Sinif { get; set; } = string.Empty;

        // CV zorunlu değil, isteğe bağlı
        public IFormFile? CVDosyasi { get; set; }
        public IFormFile? FotografDosyasi { get; set; }
    }
}