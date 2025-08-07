using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerTakipSistemi.Models.ViewModels;
using StajyerTakipSistemi.Models;
using StajyerTakipSistemi.Data;

namespace StajyerTakipSistemi.Controllers

{
    public class EgitmenController : Controller
    {
        private readonly StajyerTakipDbContext _context;

        public EgitmenController(StajyerTakipDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        public async Task<IActionResult> StajyerListesi()
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // GERÇEKLEŞTİRİLDİ: Database'den onaylı stajyerleri çek
            var stajyerler = await GetGercekStajyerler();

            return View(stajyerler);
        }

        public async Task<IActionResult> BasvuruDegerlendirme()
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // GERÇEKLEŞTİRİLDİ: Database'den bekleyen başvuruları çek
            var bekleyenBasvurular = await GetGercekBekleyenBasvurular();

            return View(bekleyenBasvurular);
        }

        [HttpPost]
        public async Task<IActionResult> BasvuruOnayla(int userId)
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // GERÇEKLEŞTİRİLDİ: Gerçek database güncellemesi
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.BasvuruDurumu = "Onaylandi";
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"{user.AdSoyad} adlı kişinin başvurusu onaylandı!";
                    Console.WriteLine($"Başvuru onaylandı: {user.AdSoyad} ({user.Email})");
                }
                else
                {
                    TempData["Error"] = "Kullanıcı bulunamadı!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Başvuru onaylanırken bir hata oluştu.";
                Console.WriteLine($"Başvuru onaylama hatası: {ex.Message}");
            }

            return RedirectToAction("BasvuruDegerlendirme");
        }

        [HttpPost]
        public async Task<IActionResult> BasvuruReddet(int userId, string redNedeni)
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // GERÇEKLEŞTİRİLDİ: Gerçek database güncellemesi
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.BasvuruDurumu = $"Reddedildi: {redNedeni}";
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"{user.AdSoyad} adlı kişinin başvurusu reddedildi.";
                    Console.WriteLine($"Başvuru reddedildi: {user.AdSoyad} - Neden: {redNedeni}");
                }
                else
                {
                    TempData["Error"] = "Kullanıcı bulunamadı!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Başvuru reddedilirken bir hata oluştu.";
                Console.WriteLine($"Başvuru reddetme hatası: {ex.Message}");
            }

            return RedirectToAction("BasvuruDegerlendirme");
        }

        public async Task<IActionResult> BasvuruDetay(int id)
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // GERÇEKLEŞTİRİLDİ: Database'den başvuru detayını çek
            var basvuru = await GetBasvuruDetay(id);

            if (basvuru == null)
            {
                return NotFound();
            }

            return View(basvuru);
        }

        public async Task<IActionResult> OdevAtama()
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // GERÇEKLEŞTİRİLDİ: Database'den ödevleri çek
            var mevcutOdevler = await GetGercekOdevler();

            return View(mevcutOdevler);
        }

        public async Task<IActionResult> YeniOdev()
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Aktif stajyerlerin listesi - sonra gerçekleştirilecek
            ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> YeniOdev(YeniOdevViewModel model)
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
                return View(model);
            }

            try
            {
                var egitmenId = HttpContext.Session.GetInt32("UserId").Value;

                // SADECE Görev
                var yeniGorev = new Gorev
                {
                    GorevAdi = model.OdevAdi,
                    Aciklama = model.Aciklama,
                    TeslimTarihi = model.TeslimTarihi,        
                    ZorlukSeviyesi = model.ZorlukSeviyesi,    
                    OlusturmaTarihi = DateTime.Now,
                    EgitmenId = egitmenId
                };

                _context.Gorevler.Add(yeniGorev);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Görev oluşturuldu: {yeniGorev.GorevAdi} (ID: {yeniGorev.Id})");
                Console.WriteLine($"✅ {model.SeciliStajyerler.Count} stajyere atanması planlandı (geçici)");

                TempData["Success"] = $"'{model.OdevAdi}' ödevi başarıyla oluşturuldu!";
                return RedirectToAction("OdevAtama");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ÖDEV OLUŞTURMA HATASI: {ex.Message}");
                TempData["Error"] = "Ödev oluşturulurken bir hata oluştu.";

                ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
                return View(model);
            }
        }
        // ===== GERÇEKLEŞTİRİLEN METODLAR =====

        private async Task<List<BasvuruDegerlendirmeViewModel>> GetGercekBekleyenBasvurular()
        {
            try
            {
                var bekleyenUsers = await _context.Users
                    .Where(u => u.UserType == UserType.Stajyer && u.BasvuruDurumu.Contains("Bekle"))
                    .ToListAsync(); // Önce sadece raw data çek

                var result = bekleyenUsers.Select(u => new BasvuruDegerlendirmeViewModel
                {
                    Id = u.Id,
                    AdSoyad = u.AdSoyad ?? "Bilinmeyen",
                    Email = u.Email ?? "",
                    Telefon = u.Telefon ?? "",
                    OkulAdi = u.OkulAdi ?? "",
                    Bolum = u.Bolum ?? "",
                    Sinif = u.Sinif ?? "",
                    StajTuru = u.StajTuru ?? "",
                    StajBaslangicTarihi = u.StajBaslangicTarihi ?? DateTime.Now,
                    StajBitisTarihi = u.StajBitisTarihi ?? DateTime.Now.AddDays(60),
                    BasvuruTarihi = u.BasvuruTarihi ?? DateTime.Now,
                    CVDosyaYolu = u.CVDosyaYolu,
                    FotografDosyaYolu = u.FotografDosyaYolu,
                    BasvuruNotu = "Test notu"
                }).ToList();

                Console.WriteLine($"✅ SUCCESS: {result.Count} başvuru bulundu!");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 HATA: {ex.Message}");
                return new List<BasvuruDegerlendirmeViewModel>();
            }
        }

        private async Task<List<StajyerListeViewModel>> GetGercekStajyerler()
        {
            try
            {
                var stajyerlar = await _context.Users
                    .Where(u => u.UserType == UserType.Stajyer && u.BasvuruDurumu.Contains("Onay"))
                    .ToListAsync();

                var result = stajyerlar.Select(u => new StajyerListeViewModel
                {
                    Id = u.Id,
                    AdSoyad = u.AdSoyad ?? "",
                    Email = u.Email ?? "",
                    Telefon = u.Telefon ?? "",
                    OkulAdi = u.OkulAdi ?? "",
                    Bolum = u.Bolum ?? "",
                    StajTuru = u.StajTuru ?? "",
                    BaslangicTarihi = u.StajBaslangicTarihi ?? DateTime.Now,
                    BitisTarihi = u.StajBitisTarihi ?? DateTime.Now.AddDays(60),
                    Durum = "Aktif",
                    IlerlemeYuzdesi = 75 // Geçici
                }).ToList();

                Console.WriteLine($"✅ STAJYER LİSTESİ: {result.Count} onaylı stajyer bulundu!");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 STAJYER LİSTESİ HATASI: {ex.Message}");
                return new List<StajyerListeViewModel>();
            }
        }

        private async Task<BasvuruDegerlendirmeViewModel?> GetBasvuruDetay(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.UserType == UserType.Stajyer)
                    .FirstOrDefaultAsync();

                if (user == null) return null;

                return new BasvuruDegerlendirmeViewModel
                {
                    Id = user.Id,
                    AdSoyad = user.AdSoyad ?? "",
                    Email = user.Email ?? "",
                    Telefon = user.Telefon ?? "",
                    OkulAdi = user.OkulAdi ?? "",
                    Bolum = user.Bolum ?? "",
                    Sinif = user.Sinif ?? "",
                    StajTuru = user.StajTuru ?? "",
                    StajBaslangicTarihi = user.StajBaslangicTarihi ?? DateTime.Now,
                    StajBitisTarihi = user.StajBitisTarihi ?? DateTime.Now.AddDays(60),
                    BasvuruTarihi = user.BasvuruTarihi ?? user.KayitTarihi ?? DateTime.Now,
                    CVDosyaYolu = user.CVDosyaYolu,
                    FotografDosyaYolu = user.FotografDosyaYolu,
                    BasvuruNotu = GetBasvuruNotu(user.Bolum)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Başvuru detayı çekilirken hata: {ex.Message}");
                return null;
            }
        }
        private async Task<List<object>> GetGercekAktifStajyerler()
        {
            try
            {
                var aktifStajyerler = await _context.Users
                    .Where(u => u.UserType == UserType.Stajyer && u.BasvuruDurumu.Contains("Onay"))
                    .Select(u => new { Id = u.Id, AdSoyad = u.AdSoyad })
                    .ToListAsync();

                Console.WriteLine($"✅ {aktifStajyerler.Count} aktif stajyer bulundu ödev atama için");

                return aktifStajyerler.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 AKTİF STAJYER LİSTESİ HATASI: {ex.Message}");
                return new List<object>();
            }
        }

        // Helper metodlar
        private string GetStajyerDurumu(DateTime? baslangic, DateTime? bitis)
        {
            if (!baslangic.HasValue || !bitis.HasValue) return "Belirsiz";

            var now = DateTime.Now.Date;
            if (now < baslangic.Value.Date) return "Başlamamış";
            if (now > bitis.Value.Date) return "Tamamlandı";
            return "Aktif";
        }

        private int GetIlerlemeYuzdesi(DateTime? baslangic, DateTime? bitis)
        {
            if (!baslangic.HasValue || !bitis.HasValue) return 0;

            var now = DateTime.Now.Date;
            if (now <= baslangic.Value.Date) return 0;
            if (now >= bitis.Value.Date) return 100;

            var toplamGun = (bitis.Value.Date - baslangic.Value.Date).Days;
            var gecenGun = (now - baslangic.Value.Date).Days;

            return toplamGun > 0 ? (int)((double)gecenGun / toplamGun * 100) : 0;
        }

        private string GetBasvuruNotu(string? bolum)
        {
            return bolum switch
            {
                "Bilgisayar Mühendisliği" => "Frontend ve backend geliştirme konularında deneyim edinmek istiyor.",
                "Yazılım Mühendisliği" => "Modern web teknolojileri ve proje yönetimi konularında gelişim hedefliyor.",
                "Endüstri Mühendisliği" => "Veri analizi ve süreç optimizasyonu alanlarında çalışmak istiyor.",
                _ => "Staj sürecinde kendini geliştirmeyi ve deneyim kazanmayı hedefliyor."
            };
        }

        // ===== MOCK METODLAR (Geçici - sonra silinecek) =====

        private async Task<List<OdevAtamaViewModel>> GetGercekOdevler()
        {
            try
            {
                var odevler = await _context.Gorevler
                    .Select(g => new OdevAtamaViewModel
                    {
                        Id = g.Id,
                        OdevAdi = g.GorevAdi ?? "",
                        Aciklama = g.Aciklama ?? "",
                        OlusturmaTarihi = g.OlusturmaTarihi,
                        TeslimTarihi = g.TeslimTarihi, 
                        ZorlukSeviyesi = g.ZorlukSeviyesi, 
                        AtananStajyerAdlari = new List<string> { "Geçici" },
                        TamamlananSayi = 0,
                        ToplamAtanan = 1,
                        Durum = "Aktif"
                    })
                    .OrderByDescending(o => o.OlusturmaTarihi)
                    .ToListAsync();

                Console.WriteLine($"✅ {odevler.Count} ödev bulundu database'den");
                return odevler;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ÖDEV LİSTESİ HATASI: {ex.Message}");
                return new List<OdevAtamaViewModel>();
            }
        }

        private string GetOdevDurumu(DateTime olusturmaTarihi)
        {
            var gecenGun = (DateTime.Now - olusturmaTarihi).Days;
            if (gecenGun < 3) return "Aktif";
            if (gecenGun < 7) return "Yakın";
            return "Süresi Doldu";
        }

        private List<object> GetMockAktifStajyerler()
        {
            return new List<object>
            {
                new { Id = 1, AdSoyad = "Test Stajyer" }
            };
        }

        // ÖDEV BUTON METODLARI

        public async Task<IActionResult> OdevDetay(int id)
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var gorev = await _context.Gorevler.FindAsync(id);
                if (gorev == null)
                {
                    TempData["Error"] = "Ödev bulunamadı!";
                    return RedirectToAction("OdevAtama");
                }

                // Geçici: Basit bilgi göster
                TempData["Info"] = $"Ödev Detayı: {gorev.GorevAdi} - {gorev.Aciklama} - Zorluk: {gorev.ZorlukSeviyesi} - Teslim: {gorev.TeslimTarihi:dd/MM/yyyy}";
                Console.WriteLine($"🔍 Ödev detay görüntülendi: {gorev.GorevAdi}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ödev detayı görüntülenirken hata oluştu.";
                Console.WriteLine($"Ödev detay hatası: {ex.Message}");
            }

            return RedirectToAction("OdevAtama");
        }

        public async Task<IActionResult> OdevDuzenle(int id)
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var gorev = await _context.Gorevler.FindAsync(id);
                if (gorev == null)
                {
                    TempData["Error"] = "Düzenlenecek ödev bulunamadı!";
                    return RedirectToAction("OdevAtama");
                }

                // Geçici: Bilgi göster, gerçek düzenleme sayfası sonra yapılacak
                TempData["Info"] = $"'{gorev.GorevAdi}' ödevi düzenleme özelliği yakında eklenecek!";
                Console.WriteLine($"✏️ Ödev düzenleme isteği: {gorev.GorevAdi}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ödev bilgileri alınırken hata oluştu.";
                Console.WriteLine($"Ödev düzenleme hatası: {ex.Message}");
            }

            return RedirectToAction("OdevAtama");
        }

        [HttpPost]
        public async Task<IActionResult> OdevSil(int id)
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var gorev = await _context.Gorevler.FindAsync(id);
                if (gorev == null)
                {
                    TempData["Error"] = "Silinecek ödev bulunamadı!";
                    return RedirectToAction("OdevAtama");
                }

                var odevAdi = gorev.GorevAdi;
                _context.Gorevler.Remove(gorev);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"'{odevAdi}' ödevi başarıyla silindi!";
                Console.WriteLine($"🗑️ Ödev silindi: {odevAdi}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ödev silinirken hata oluştu.";
                Console.WriteLine($"Ödev silme hatası: {ex.Message}");
            }

            return RedirectToAction("OdevAtama");
        }
    }
}