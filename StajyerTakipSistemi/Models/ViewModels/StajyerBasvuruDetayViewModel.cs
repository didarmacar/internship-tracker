using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models.ViewModels
{
    public class StajyerBasvuruDetayViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad gereklidir")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        // E-mail alanı kaldırıldı - değiştirilemez olduğu için
        public string Email { get; set; } = string.Empty; // Sadece görüntüleme için

        [Display(Name = "Mevcut Şifre")]
        [DataType(DataType.Password)]
        public string? MevcutSifre { get; set; }

        [Display(Name = "Yeni Şifre")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır.", MinimumLength = 6)]
        public string? YeniSifre { get; set; }

        [Display(Name = "Yeni Şifre Tekrar")]
        [DataType(DataType.Password)]
        [Compare("YeniSifre", ErrorMessage = "Şifreler eşleşmiyor")]
        public string? YeniSifreTekrar { get; set; }

        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Okul adı gereklidir")]
        [Display(Name = "Okul Adı")]
        public string OkulAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bölüm bilgisi gereklidir")]
        [Display(Name = "Bölüm")]
        public string Bolum { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sınıf bilgisi gereklidir")]
        [Display(Name = "Sınıf")]
        public string Sinif { get; set; } = string.Empty;

        [Required(ErrorMessage = "Staj türü seçimi gereklidir")]
        [Display(Name = "Staj Türü")]
        public string StajTuru { get; set; } = string.Empty;

        [Required(ErrorMessage = "Staj başlangıç tarihi gereklidir")]
        [DataType(DataType.Date)]
        [Display(Name = "Staj Başlangıç Tarihi")]
        public DateTime StajBaslangicTarihi { get; set; }

        [Required(ErrorMessage = "Staj bitiş tarihi gereklidir")]
        [DataType(DataType.Date)]
        [Display(Name = "Staj Bitiş Tarihi")]
        public DateTime StajBitisTarihi { get; set; }

        // Mevcut dosya yolları
        public string? CVDosyaYolu { get; set; }
        public string? FotografDosyaYolu { get; set; }

        // Yeni dosyalar (isteğe bağlı)
        [Display(Name = "CV Dosyası (Değiştirmek için)")]
        public IFormFile? YeniCVDosyasi { get; set; }

        [Display(Name = "Fotoğraf (Değiştirmek için)")]
        public IFormFile? YeniFotografDosyasi { get; set; }

        // Bilgi amaçlı alanlar
        public DateTime BasvuruTarihi { get; set; }
        public string BasvuruDurumu { get; set; } = string.Empty;
    }
}