using StajyerTakipSistemi.Models;

namespace StajyerTakipSistemi.Data
{
    public class DbInitializer
    {
        public static void Initialize(StajyerTakipDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;
            }

            try
            {
                // Sadece test eğitmeni ekle
                var users = new User[]
                {
                    new User
                    {
                        AdSoyad = "Alper",
                        Email = "egitmen@test.com",
                        Sifre = BCrypt.Net.BCrypt.HashPassword("123456"),
                        UserType = UserType.Mentor,
                        KayitTarihi = DateTime.Now,
                        BasvuruDurumu = "Aktif"
                    }
                };

                foreach (var user in users)
                {
                    context.Users.Add(user);
                }

                context.SaveChanges();
                Console.WriteLine("✅ Database initialized with test mentor only. Real applications will be shown.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database initialization error: {ex.Message}");
                throw;
            }
        }
    }
}