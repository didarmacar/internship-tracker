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

                // Add sample training data
                if (!context.Egitimler.Any())
                {
                    var egitimler = new Egitim[]
                    {
                        new Egitim
                        {
                            EgitimAdi = "React Hooks",
                            Baslik = "React Hooks Eğitimi",
                            Tarih = DateTime.Today,
                            BaslangicSaati = new TimeSpan(10, 0, 0),
                            BitisSaati = new TimeSpan(11, 30, 0),
                            Aciklama = "React hooks konularının detaylı anlatımı"
                        },
                        new Egitim
                        {
                            EgitimAdi = "Proje Değerlendirme",
                            Baslik = "Proje Değerlendirme Toplantısı", 
                            Tarih = DateTime.Today,
                            BaslangicSaati = new TimeSpan(14, 0, 0),
                            BitisSaati = new TimeSpan(15, 0, 0),
                            Aciklama = "Haftalık proje ilerlemelerinin değerlendirilmesi"
                        },
                        new Egitim
                        {
                            EgitimAdi = "Database Design",
                            Baslik = "Veritabanı Tasarımı",
                            Tarih = DateTime.Today.AddDays(1),
                            BaslangicSaati = new TimeSpan(9, 0, 0),
                            BitisSaati = new TimeSpan(10, 30, 0),
                            Aciklama = "İlişkisel veritabanı tasarım prensipleri"
                        }
                    };

                    foreach (var egitim in egitimler)
                    {
                        context.Egitimler.Add(egitim);
                    }

                    context.SaveChanges();
                    Console.WriteLine("Sample training data created!");
                }

                // Add sample tasks and assignments if we have users
                var mentorUser = context.Users.FirstOrDefault(u => u.UserType == UserType.Mentor);
                var internUser = context.Users.FirstOrDefault(u => u.UserType == UserType.Stajyer);

                if (mentorUser != null && internUser != null && !context.Gorevler.Any())
                {
                    // Create Stajyer entity for the intern user if it doesn't exist
                    var stajyer = context.Stajyerler.FirstOrDefault(s => s.UserId == internUser.Id);
                    if (stajyer == null)
                    {
                        stajyer = new Stajyer
                        {
                            UserId = internUser.Id,
                            Telefon = "555-1234",
                            OkulAdi = "Test Üniversitesi",
                            Bolum = "Bilgisayar Mühendisliği", 
                            Sinif = "4",
                            StajTuru = StajTuru.Zorunlu,
                            BaslangicTarihi = DateTime.Now.AddDays(-30),
                            BitisTarihi = DateTime.Now.AddDays(30),
                            Durum = StajDurumu.Aktif
                        };
                        context.Stajyerler.Add(stajyer);
                        context.SaveChanges();
                    }

                    // Add sample tasks
                    var gorevler = new Gorev[]
                    {
                        new Gorev
                        {
                            GorevAdi = "JavaScript Temelleri Projesi",
                            Aciklama = "Temel JavaScript kavramlarını içeren bir proje geliştirin",
                            TeslimTarihi = DateTime.Now.AddDays(7),
                            ZorlukSeviyesi = "Başlangıç",
                            EgitmenId = mentorUser.Id,
                            OlusturmaTarihi = DateTime.Now.AddDays(-2)
                        },
                        new Gorev
                        {
                            GorevAdi = "HTML/CSS Website Tasarımı",
                            Aciklama = "Responsive bir website tasarlayın",
                            TeslimTarihi = DateTime.Now.AddDays(-1), // Completed task
                            ZorlukSeviyesi = "Başlangıç",
                            EgitmenId = mentorUser.Id,
                            OlusturmaTarihi = DateTime.Now.AddDays(-10)
                        },
                        new Gorev
                        {
                            GorevAdi = "Veritabanı Şema Tasarımı",
                            Aciklama = "E-ticaret sistemi için veritabanı şeması oluşturun",
                            TeslimTarihi = DateTime.Now.AddDays(-3), // Overdue task
                            ZorlukSeviyesi = "Orta",
                            EgitmenId = mentorUser.Id,
                            OlusturmaTarihi = DateTime.Now.AddDays(-15)
                        }
                    };

                    foreach (var gorev in gorevler)
                    {
                        context.Gorevler.Add(gorev);
                    }
                    context.SaveChanges();

                    // Assign tasks to intern
                    var savedGorevler = context.Gorevler.ToList();
                    foreach (var gorev in savedGorevler)
                    {
                        var assignment = new StajyerGorev
                        {
                            StajyerId = stajyer.Id,
                            GorevId = gorev.Id,
                            AtamaTarihi = gorev.OlusturmaTarihi,
                            Tamamlandi = gorev.GorevAdi == "HTML/CSS Website Tasarımı", // Mark one as completed
                            TamamlanmaTarihi = gorev.GorevAdi == "HTML/CSS Website Tasarımı" ? DateTime.Now.AddDays(-1) : null
                        };
                        context.StajyerGorevler.Add(assignment);
                    }

                    context.SaveChanges();
                    Console.WriteLine("Sample tasks and assignments created!");
                }

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