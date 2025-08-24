namespace StajyerTakipSistemi.Models
{
    public class StajyerGorev
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int GorevId { get; set; }
        public Gorev Gorev { get; set; }

        public DateTime AtamaTarihi { get; set; } = DateTime.Now;
        public DateTime? TamamlanmaTarihi { get; set; }
        public bool Tamamlandi { get; set; } = false;
    }
}