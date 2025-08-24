using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad gereklidir")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; }

        [Required(ErrorMessage = "E-mail adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-mail adresi giriniz")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor")]
        public string SifreTekrar { get; set; }

        // Geçici olarak geri eklendi - eğitmen hesabı oluşturmak için
        [Required(ErrorMessage = "Kullanıcı türü seçimi gereklidir")]
        [Display(Name = "Kullanıcı Türü")]
        public UserType UserType { get; set; } = UserType.Stajyer;
    }
}