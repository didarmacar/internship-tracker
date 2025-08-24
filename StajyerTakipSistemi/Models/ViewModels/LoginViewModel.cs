using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Sifre { get; set; }

        public UserType UserType { get; set; }
    }
}