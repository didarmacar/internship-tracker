using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerTakipSistemi.Data;
using StajyerTakipSistemi.Models;
using StajyerTakipSistemi.Models.ViewModels;

namespace StajyerTakipSistemi.Controllers
{
    public class StajyerController : Controller
    {
        private readonly StajyerTakipDbContext _context;

        public StajyerController(StajyerTakipDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Kullanıcının başvuru durumunu kontrol et
            var basvuruDurumu = GetUserBasvuruDurumu(userId.Value);

            if (basvuruDurumu == "Yapilmamis")
            {
                return View("BasvuruIndex");
            }
            else if (basvuruDurumu == "Beklemede")
            {
                return View("BasvuruBeklemede");
            }
            else if (basvuruDurumu == "Reddedildi")
            {
                return View("BasvuruReddedildi");
            }
            else if (basvuruDurumu == "Onaylandi")
            {
                // Mevcut dashboard kodu
                string userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";
                var dashboardData = new StajyerDashboardViewModel
                {
                    KullaniciAdi = userName,
                    KullaniciInitials = GetInitials(userName),
                    BekleyenOdevler = GetSampleTasks(),
                    BugunEgitimleri = GetBugunEgitimleri(),
                    GenelIlerleme = 68,
                    BuHaftaIlerleme = 85,
                    SonAktiviteler = GetRecentActivities()
                };
                return View("Index", dashboardData);
            }

            return View("BasvuruIndex");
        }

        public IActionResult Odevlerim()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Başvuru durumu kontrolü
            var basvuruDurumu = GetUserBasvuruDurumu(userId.Value);
            if (basvuruDurumu != "Onaylandi")
            {
                return RedirectToAction("Index");
            }

            string userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";

            // Örnek ödev verisi
            var odevler = new List<dynamic>
            {
                new { GorevAdi = "JavaScript Temelleri", Durum = "Bekliyor", TeslimTarihi = "3 gün kaldı", DurumClass = "text-warning" },
                new { GorevAdi = "HTML/CSS Website", Durum = "Tamamlandı", TeslimTarihi = "2 gün önce", DurumClass = "text-success" },
                new { GorevAdi = "React Projesi", Durum = "Devam Ediyor", TeslimTarihi = "1 hafta kaldı", DurumClass = "text-info" },
                new { GorevAdi = "Database Tasarımı", Durum = "Gecikmiş", TeslimTarihi = "1 gün gecikti", DurumClass = "text-danger" }
            };

            ViewBag.Odevler = odevler;
            ViewBag.UserName = userName;

            return View();
        }

        public IActionResult Takvim()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Başvuru durumu kontrolü
            var basvuruDurumu = GetUserBasvuruDurumu(userId.Value);
            if (basvuruDurumu != "Onaylandi")
            {
                return RedirectToAction("Index");
            }

            string userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";

            // Takvim verilerini hazırla
            var takvimData = GetTakvimVerileri();

            ViewBag.UserName = userName;
            ViewBag.TakvimData = takvimData;

            return View();
        }
        public IActionResult Mesajlar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Başvuru durumu kontrolü
            var basvuruDurumu = GetUserBasvuruDurumu(userId.Value);
            if (basvuruDurumu != "Onaylandi")
            {
                return RedirectToAction("Index");
            }

            string userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";
            ViewBag.UserName = userName;

            return View();
        }

        public IActionResult KahveKutusu()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Başvuru durumu kontrolü
            var basvuruDurumu = GetUserBasvuruDurumu(userId.Value);
            if (basvuruDurumu != "Onaylandi")
            {
                return RedirectToAction("Index");
            }

            string userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";
            ViewBag.UserName = userName;

            return View();
        }

        public IActionResult Basvuru()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            string userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";
            ViewBag.UserName = userName;

            return View();
        }

        private string GetUserBasvuruDurumu(int userId)
        {
            return "Onaylandi";
        }

        [HttpPost]
        public async Task<IActionResult> BasvuruGonder(StajyerBasvuruViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View("BasvuruIndex", model);
            }

            try
            {
                string cvPath = null, fotoPath = null;

                if (model.CVDosyasi != null)
                {
                    cvPath = await SaveFile(model.CVDosyasi, "cv");
                }

                if (model.FotografDosyasi != null)
                {
                    fotoPath = await SaveFile(model.FotografDosyasi, "foto");
                }

                await UpdateUserBasvuru(userId.Value, model, cvPath, fotoPath);

                TempData["SuccessMessage"] = "Başvurunuz başarıyla gönderildi!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Başvuru gönderilirken bir hata oluştu.";
                return View("BasvuruIndex", model);
            }
        }

        private async Task<string> SaveFile(IFormFile file, string type)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", type);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{type}/{fileName}";
        }

        private async Task UpdateUserBasvuru(int userId, StajyerBasvuruViewModel model, string cvPath, string fotoPath)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Telefon = model.Telefon;
                user.StajTuru = model.StajTuru;
                user.StajBaslangicTarihi = model.StajBaslangicTarihi;
                user.StajBitisTarihi = model.StajBitisTarihi;
                user.OkulAdi = model.OkulAdi;
                user.Bolum = model.Bolum;
                user.Sinif = model.Sinif ?? string.Empty; // Null kontrolü eklendi
                user.CVDosyaYolu = cvPath;
                user.FotografDosyaYolu = fotoPath;
                user.BasvuruDurumu = "Beklemede";
                user.BasvuruTarihi = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }

        private dynamic GetTakvimVerileri()
        {
            return new
            {
                BuAyEtkinlikler = new[]
                {
                    new { Tarih = "1 Ağustos", Gun = "Perşembe", Etkinlik = "JavaScript Temelleri Eğitimi", Saat = "10:00-12:00", Tip = "egitim" },
                    new { Tarih = "3 Ağustos", Gun = "Cumartesi", Etkinlik = "Proje Teslimi", Saat = "23:59", Tip = "odev" },
                    new { Tarih = "5 Ağustos", Gun = "Pazartesi", Etkinlik = "Mentor Görüşmesi", Saat = "14:00-15:00", Tip = "toplanti" },
                    new { Tarih = "8 Ağustos", Gun = "Perşembe", Etkinlik = "React Hooks Workshop", Saat = "09:00-17:00", Tip = "workshop" },
                    new { Tarih = "12 Ağustos", Gun = "Pazartesi", Etkinlik = "Haftalık Değerlendirme", Saat = "16:00-17:00", Tip = "toplanti" },
                    new { Tarih = "15 Ağustos", Gun = "Perşembe", Etkinlik = "Database Tasarımı Projesi", Saat = "23:59", Tip = "odev" }
                },
                BugununEtkinlikleri = new[]
                {
                    new { Saat = "10:00", Etkinlik = "Daily Standup", Tip = "toplanti" },
                    new { Saat = "14:00", Etkinlik = "Code Review", Tip = "egitim" },
                    new { Saat = "16:00", Etkinlik = "React Projesi Çalışması", Tip = "odev" }
                }
            };
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "ST";

            var parts = fullName.Split(' ');
            if (parts.Length >= 2)
            {
                return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
            }
            return fullName.Substring(0, Math.Min(2, fullName.Length)).ToUpper();
        }

        private List<OdevViewModel> GetSampleTasks()
        {
            // Örnek görevler
            return new List<OdevViewModel>
            {
                new OdevViewModel { GorevAdi = "JavaScript Temelleri Projesi", Durum = "Bekliyor" },
                new OdevViewModel { GorevAdi = "HTML/CSS Website Tasarımı", Durum = "Tamamlandı" },
                new OdevViewModel { GorevAdi = "Veritabanı Şema Tasarımı", Durum = "Gecikmiş" }
            };
        }

        private List<EgitimViewModel> GetBugunEgitimleri()
        {
            return new List<EgitimViewModel>
            {
                new EgitimViewModel { Saat = "10:00 - 11:30", Baslik = "React Hooks Eğitimi" },
                new EgitimViewModel { Saat = "14:00 - 15:00", Baslik = "Proje Değerlendirme Toplantısı" }
            };
        }

        private List<AktiviteViewModel> GetRecentActivities()
        {
            return new List<AktiviteViewModel>
            {
                new AktiviteViewModel
                {
                    Icon = "✅",
                    IconClass = "activity-completed",
                    Baslik = "CSS Grid projesi tamamlandı",
                    Aciklama = "Günlük rapor gönderildi"
                },
                new AktiviteViewModel
                {
                    Icon = "📖",
                    IconClass = "activity-progress",
                    Baslik = "Mentor ile görüşme yapıldı",
                    Aciklama = "JavaScript makale okundu"
                }
            };
        }
    }

    // ViewModel sınıfları
    public class StajyerDashboardViewModel
    {
        public string KullaniciAdi { get; set; } = string.Empty;
        public string KullaniciInitials { get; set; } = string.Empty;
        public List<OdevViewModel> BekleyenOdevler { get; set; } = new List<OdevViewModel>();
        public List<EgitimViewModel> BugunEgitimleri { get; set; } = new List<EgitimViewModel>();
        public int GenelIlerleme { get; set; }
        public int BuHaftaIlerleme { get; set; }
        public List<AktiviteViewModel> SonAktiviteler { get; set; } = new List<AktiviteViewModel>();
    }

    public class OdevViewModel
    {
        public string GorevAdi { get; set; } = string.Empty;
        public string Durum { get; set; } = string.Empty;
    }

    public class EgitimViewModel
    {
        public string Saat { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
    }

    public class AktiviteViewModel
    {
        public string Icon { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
    }
}