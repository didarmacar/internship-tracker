using StajyerTakipSistemi.Models;

namespace StajyerTakipSistemi.Data
{
    public class DbInitializer
    {
        public static void Initialize(StajyerTakipDbContext context)
        {
            context.Database.EnsureCreated();

            // Zaten veri varsa çık
            if (context.Users.Any())
            {
                return;
            }

            try
            {
                // Test kullanıcıları - minimal versiyon
                var users = new User[]
                {
                    new User
                    {
                        AdSoyad = "Test Eğitmen",
                        Email = "egitmen@test.com",
                        Sifre = BCrypt.Net.BCrypt.HashPassword("123456"),
                        UserType = UserType.Mentor,
                        KayitTarihi = DateTime.Now,
                        BasvuruDurumu = "Aktif"
                    },
                    new User
                    {
                        AdSoyad = "Test Stajyer",
                        Email = "stajyer@test.com",
                        Sifre = BCrypt.Net.BCrypt.HashPassword("123456"),
                        UserType = UserType.Stajyer,
                        KayitTarihi = DateTime.Now,
                        BasvuruDurumu = "Onaylandi"
                    }
                };

                foreach (var user in users)
                {
                    context.Users.Add(user);
                    Console.WriteLine($"Adding user: {user.Email}");
                }

                context.SaveChanges();
                Console.WriteLine("Test users created successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DbInitializer error: {ex.Message}");
                throw;
            }
        }
    }
}