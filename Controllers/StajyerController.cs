using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerTakipSistemi.Data;
using StajyerTakipSistemi.Models;
using StajyerTakipSistemi.Models.ViewModels;
using StajyerTakipSistemi.Services;

namespace StajyerTakipSistemi.Controllers
{
    public class StajyerController : Controller
    {
        private readonly StajyerTakipDbContext _context;
        private readonly IStajyerService _stajyerService;

        public StajyerController(StajyerTakipDbContext context, IStajyerService stajyerService)
        {
            _context = context;
            _stajyerService = stajyerService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Kullanıcının başvuru durumunu kontrol et
            var basvuruDurumu = await _stajyerService.GetUserBasvuruDurumu(userId.Value);

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
                // Dashboard with real data from service
                string userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";
                var dashboardData = new StajyerDashboardViewModel
                {
                    KullaniciAdi = userName,
                    KullaniciInitials = GetInitials(userName),
                    BekleyenOdevler = await _stajyerService.GetStajyerOdevleri(userId.Value),
                    BugunEgitimleri = await _stajyerService.GetStajyerEgitimleri(userId.Value),
                    GenelIlerleme = await _stajyerService.CalculateGenelIlerleme(userId.Value),
                    BuHaftaIlerleme = await _stajyerService.CalculateBuHaftaIlerleme(userId.Value),
                    SonAktiviteler = await _stajyerService.GetStajyerAktiviteleri(userId.Value)
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
            var basvuruDurumu = await _stajyerService.GetUserBasvuruDurumu(userId.Value);
            if (basvuruDurumu != "Onaylandi")
            {
                return RedirectToAction("Index");
            }

            string userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";

            // Real assignment data from service
            var odevler = await _stajyerService.GetStajyerOdevleri(userId.Value);
            ViewBag.Odevler = odevler;
            ViewBag.UserName = userName;

            return View();
        }

        public async Task<IActionResult> Takvim()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Başvuru durumu kontrolü
            var basvuruDurumu = await _stajyerService.GetUserBasvuruDurumu(userId.Value);
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
        public async Task<IActionResult> Mesajlar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Başvuru durumu kontrolü
            var basvuruDurumu = await _stajyerService.GetUserBasvuruDurumu(userId.Value);
            if (basvuruDurumu != "Onaylandi")
            {
                return RedirectToAction("Index");
            }

            string userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";
            ViewBag.UserName = userName;

            return View();
        }

        public async Task<IActionResult> KahveKutusu()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Başvuru durumu kontrolü
            var basvuruDurumu = await _stajyerService.GetUserBasvuruDurumu(userId.Value);
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

                await _stajyerService.UpdateUserBasvuru(userId.Value, model, cvPath, fotoPath);

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


    }
}