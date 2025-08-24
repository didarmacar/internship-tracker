using System.ComponentModel.DataAnnotations;

namespace StajyerTakipSistemi.Models
{
    public class Gorev
    {
        public int Id { get; set; }

        [Required]
        public string GorevAdi { get; set; }

        public string? Aciklama { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime TeslimTarihi { get; set; }
        public string ZorlukSeviyesi { get; set; } = "";

        public int EgitmenId { get; set; }
        public User Egitmen { get; set; }

        // Artık direkt User'larla ilişki
        public List<StajyerGorev> StajyerGorevler { get; set; } = new List<StajyerGorev>();
    }
}