using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerTakipSistemi.Models.ViewModels;
using StajyerTakipSistemi.Models;
using StajyerTakipSistemi.Data;
using System.Text;
using ClosedXML.Excel;

namespace StajyerTakipSistemi.Controllers

{
    public class EgitmenController : Controller
    {
        private readonly StajyerTakipDbContext _context;

        public EgitmenController(StajyerTakipDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // Gerçek istatistikleri hesapla
                var toplamStajyer = await _context.Users
                    .CountAsync(u => u.UserType == UserType.Stajyer);

                var aktifStajyer = await _context.Users
                    .CountAsync(u => u.UserType == UserType.Stajyer && u.BasvuruDurumu == "Onaylandi");

                var tamamlananStajyer = await _context.Users
                    .CountAsync(u => u.UserType == UserType.Stajyer &&
                                   u.StajBitisTarihi.HasValue &&
                                   u.StajBitisTarihi.Value < DateTime.Now);

                var bekleyenBasvurular = await _context.Users
                    .CountAsync(u => u.UserType == UserType.Stajyer && u.BasvuruDurumu == "Beklemede");

                var bekleyenOdevTeslimleri = await _context.OdevTeslimler
                    .CountAsync(ot => ot.Durum == TeslimDurumu.Beklemede);

                var toplamOdev = await _context.Gorevler.CountAsync();

                // ViewBag'e verileri aktar
                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                ViewBag.ToplamStajyer = toplamStajyer;
                ViewBag.AktifStajyer = aktifStajyer;
                ViewBag.TamamlananStajyer = tamamlananStajyer;
                ViewBag.BekleyenBasvurular = bekleyenBasvurular;
                ViewBag.BekleyenOdevTeslimleri = bekleyenOdevTeslimleri;
                ViewBag.ToplamOdev = toplamOdev;
                // En son onaylanan 3 stajyeri getir
                var sonOnaylananStajyerler = await _context.Users
                    .Where(u => u.UserType == UserType.Stajyer && u.BasvuruDurumu == "Onaylandi")
                    .OrderByDescending(u => u.KayitTarihi) // Kayıt tarihine göre sırala
                    .Take(3)
                    .Select(u => new {
                        u.AdSoyad,
                        OnaylanmaTarihi = u.KayitTarihi,
                        u.Bolum
                    })
                    .ToListAsync();

                ViewBag.SonOnaylananStajyerler = sonOnaylananStajyerler;

                Console.WriteLine($"📊 İstatistikler - Toplam: {toplamStajyer}, Aktif: {aktifStajyer}, Bekleyen Başvuru: {bekleyenBasvurular}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ İstatistik hesaplama hatası: {ex.Message}");

                // Hata durumunda varsayılan değerler
                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                ViewBag.ToplamStajyer = 0;
                ViewBag.AktifStajyer = 0;
                ViewBag.TamamlananStajyer = 0;
                ViewBag.BekleyenBasvurular = 0;
                ViewBag.BekleyenOdevTeslimleri = 0;
                ViewBag.ToplamOdev = 0;
            }

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
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.BasvuruDurumu = "Reddedildi";
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

        // YeniOdev POST metodunu bu temiz versiyonla değiştirin:

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

            // Stajyer seçimi kontrolü
            if (model.SeciliStajyerler == null || !model.SeciliStajyerler.Any())
            {
                ModelState.AddModelError("SeciliStajyerler", "En az bir stajyer seçmelisiniz!");
                ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
                return View(model);
            }

            try
            {
                var egitmenId = HttpContext.Session.GetInt32("UserId").Value;

                // Önce Görev oluştur
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

                foreach (var userId in model.SeciliStajyerler)
                {
                    var stajyerUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == userId && u.UserType == UserType.Stajyer);

                    if (stajyerUser != null)
                    {
                        var mevcutAtama = await _context.StajyerGorevler
                            .FirstOrDefaultAsync(sg => sg.UserId == userId && sg.GorevId == yeniGorev.Id);

                        if (mevcutAtama == null)
                        {
                            var stajyerGorev = new StajyerGorev
                            {
                                UserId = userId, // Direkt User ID
                                GorevId = yeniGorev.Id,
                                AtamaTarihi = DateTime.Now,
                                Tamamlandi = false
                            };
                            _context.StajyerGorevler.Add(stajyerGorev);
                        }
                    }
                }

                await _context.SaveChangesAsync(); // Atamaları kaydet

                TempData["Success"] = $"'{model.OdevAdi}' ödevi başarıyla oluşturuldu ve {model.SeciliStajyerler.Count} stajyere atandı!";
                return RedirectToAction("OdevAtama");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ödev oluşturulurken hata: {ex.Message}";
                ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
                return View(model);
            }
        }
        //  GERÇEKLEŞTİRİLEN METODLAR 

        private async Task<List<BasvuruDegerlendirmeViewModel>> GetGercekBekleyenBasvurular()
        {
            try
            {
                var bekleyenUsers = await _context.Users
                    .Where(u => u.UserType == UserType.Stajyer && u.BasvuruDurumu == "Beklemede")
                    .ToListAsync();

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
                    BasvuruNotu = GetBasvuruNotu(u.Bolum)
                }).ToList();

                return result;
            }
            catch (Exception)
            {
                return new List<BasvuruDegerlendirmeViewModel>();
            }
        }

        private async Task<List<StajyerListeViewModel>> GetGercekStajyerler()
        {
            try
            {
                var stajyerlar = await _context.Users
                    .Where(u => u.UserType == UserType.Stajyer && u.BasvuruDurumu == "Onaylandi")
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
                    IlerlemeYuzdesi = GetIlerlemeYuzdesi(u.StajBaslangicTarihi, u.StajBitisTarihi), // Mevcut progress bar için
                    IlerlemeMetni = GetIlerlemeMetni(u.StajBaslangicTarihi, u.StajBitisTarihi) // Yeni gün/toplam formatı
                }).ToList();

                return result;
            }
            catch (Exception )
            {
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
            catch (Exception )
            {
                return null;
            }
        }
        private async Task<List<object>> GetGercekAktifStajyerler()
        {
            try
            {
                var aktifStajyerler = await _context.Users
                    .Where(u => u.UserType == UserType.Stajyer && u.BasvuruDurumu == "Onaylandi")
                    .Select(u => new { Id = u.Id, AdSoyad = u.AdSoyad })
                    .ToListAsync();

                return aktifStajyerler.Cast<object>().ToList();
            }
            catch (Exception )
            {
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

        private string GetIlerlemeMetni(DateTime? baslangic, DateTime? bitis)
        {
            if (!baslangic.HasValue || !bitis.HasValue) return "0/0";

            var now = DateTime.Now.Date;
            var baslangicTarih = baslangic.Value.Date;
            var bitisTarih = bitis.Value.Date;

            // Toplam staj günü
            var toplamGun = (bitisTarih - baslangicTarih).Days + 1; // +1 çünkü ilk gün de dahil

            // Geçen gün sayısı
            int gecenGun;
            if (now <= baslangicTarih)
            {
                gecenGun = 0; // Henüz başlamamış
            }
            else if (now >= bitisTarih)
            {
                gecenGun = toplamGun; // Tamamlanmış
            }
            else
            {
                gecenGun = (now - baslangicTarih).Days + 1; // +1 çünkü bugün de dahil
            }

            return $"{gecenGun}/{toplamGun}";
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


        private async Task<List<OdevAtamaViewModel>> GetGercekOdevler()
        {
            try
            {
                var odevler = await _context.Gorevler
                    .Include(g => g.StajyerGorevler)
                        .ThenInclude(sg => sg.User) 
                    .ToListAsync();

                var result = odevler.Select(g => new OdevAtamaViewModel
                {
                    Id = g.Id,
                    OdevAdi = g.GorevAdi ?? "",
                    Aciklama = g.Aciklama ?? "",
                    OlusturmaTarihi = g.OlusturmaTarihi,
                    TeslimTarihi = g.TeslimTarihi,
                    ZorlukSeviyesi = g.ZorlukSeviyesi ?? "",
                    AtananStajyerAdlari = g.StajyerGorevler
                        .Where(sg => sg.User != null)
                        .Select(sg => sg.User.AdSoyad ?? "Bilinmeyen")
                        .ToList(),
                    TamamlananSayi = g.StajyerGorevler.Count(sg => sg.Tamamlandi),
                    ToplamAtanan = g.StajyerGorevler.Count,
                    Durum = GetOdevDurumu(g.TeslimTarihi)
                }).OrderByDescending(o => o.OlusturmaTarihi).ToList();

                return result;
            }
            catch (Exception )
            {
                return new List<OdevAtamaViewModel>();
            }
        }

        private string GetOdevDurumu(DateTime teslimTarihi)
        {
            var now = DateTime.Now.Date;
            var teslimTarihiDate = teslimTarihi.Date;

            if (now > teslimTarihiDate)
            {
                return "Suresi Doldu";
            }
            else if (now == teslimTarihiDate)
            {
                return "Son Gun";
            }
            else
            {
                var kalanGun = (teslimTarihiDate - now).Days;
                if (kalanGun <= 3)
                {
                    return "Yakinda Bitecek";
                }
                return "Aktif";
            }
        }

        private List<object> GetMockAktifStajyerler()
        {
            return new List<object>
            {
                new { Id = 1, AdSoyad = "Test Stajyer" }
            };
        }

        
        
        [HttpGet]
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

                // Ödev verilerini YeniOdevViewModel'e dönüştür
                var model = new YeniOdevViewModel
                {
                    OdevAdi = gorev.GorevAdi,
                    Aciklama = gorev.Aciklama,
                    TeslimTarihi = gorev.TeslimTarihi,
                    ZorlukSeviyesi = gorev.ZorlukSeviyesi,
                    // Atanmış stajyerleri al (şimdilik boş, isterseniz ekleyebiliriz)
                    SeciliStajyerler = new List<int>()
                };

                ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
                ViewBag.IsEdit = true; // Düzenleme modunda olduğunu belirt
                ViewBag.OdevId = id; // Ödev ID'sini ViewBag'e ekle

                return View("OdevDuzenle", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ödev bilgileri alınırken hata oluştu.";
                Console.WriteLine($"Ödev düzenleme hatası: {ex.Message}");
                return RedirectToAction("OdevAtama");
            }
        }

        [HttpPost]
        public async Task<IActionResult> OdevDuzenle(int id, YeniOdevViewModel model)
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
                ViewBag.IsEdit = true;
                ViewBag.OdevId = id;
                return View("OdevDuzenle", model);
            }

            try
            {
                var gorev = await _context.Gorevler.FindAsync(id);
                if (gorev == null)
                {
                    TempData["Error"] = "Güncellenecek ödev bulunamadı!";
                    return RedirectToAction("OdevAtama");
                }

                // Ödev bilgilerini güncelle
                gorev.GorevAdi = model.OdevAdi;
                gorev.Aciklama = model.Aciklama;
                gorev.TeslimTarihi = model.TeslimTarihi;
                gorev.ZorlukSeviyesi = model.ZorlukSeviyesi;

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Ödev güncellendi: {gorev.GorevAdi} (ID: {gorev.Id})");
                TempData["Success"] = $"'{model.OdevAdi}' ödevi başarıyla güncellendi!";

                return RedirectToAction("OdevAtama");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ÖDEV GÜNCELLEME HATASI: {ex.Message}");
                TempData["Error"] = "Ödev güncellenirken bir hata oluştu.";

                ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
                ViewBag.IsEdit = true;
                ViewBag.OdevId = id;
                return View("OdevDuzenle", model);
            }
        }

        // OdevSil metodu
        
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

        // 1. EgitmenController.cs - showStudents için API endpoint ekleyin:

        [HttpGet]
        public async Task<IActionResult> GetStajyerler(int gorevId)
        {
            try
            {
                var stajyerGorevler = await _context.StajyerGorevler
                    .Where(sg => sg.GorevId == gorevId)
                    .ToListAsync();

                var stajyerListesi = new List<object>();

                foreach (var sg in stajyerGorevler)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == sg.UserId);
                    if (user != null)
                    {
                        var initials = "";
                        if (!string.IsNullOrEmpty(user.AdSoyad))
                        {
                            var parts = user.AdSoyad.Split(' ');
                            if (parts.Length >= 2)
                            {
                                initials = parts[0][0].ToString() + parts[1][0].ToString();
                            }
                            else
                            {
                                initials = user.AdSoyad.Substring(0, Math.Min(2, user.AdSoyad.Length));
                            }
                        }

                        stajyerListesi.Add(new
                        {
                            id = user.Id,
                            adSoyad = user.AdSoyad ?? "Bilinmeyen",
                            initials = initials.ToUpper(),
                            pozisyon = "Frontend Developer", // Geçici
                            durum = sg.Tamamlandi ? "completed" : "pending",
                            durumText = sg.Tamamlandi ? "Tamamlandı" : "Devam Ediyor"
                        });
                    }
                }

                return Json(new { success = true, stajyerler = stajyerListesi });
            }
            catch (Exception )
            {
                return Json(new { success = false, message = "Hata oluştu" });
            }
        }
        // EgitmenController.cs içine eklenecek method

        [HttpGet]
        public async Task<IActionResult> StajyerDetay(int id)
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            try
            {
                var stajyer = await _context.Users
                    .Where(u => u.Id == id && u.UserType == UserType.Stajyer && u.BasvuruDurumu == "Onaylandi")
                    .FirstOrDefaultAsync();

                if (stajyer == null)
                {
                    TempData["Error"] = "Stajyer bulunamadı!";
                    return RedirectToAction("StajyerListesi");
                }

                // ViewModel'e dönüştür
                var detayModel = new StajyerDetayViewModel
                {
                    Id = stajyer.Id,
                    AdSoyad = stajyer.AdSoyad ?? "",
                    Email = stajyer.Email ?? "",
                    Telefon = stajyer.Telefon ?? "",
                    OkulAdi = stajyer.OkulAdi ?? "",
                    Bolum = stajyer.Bolum ?? "",
                    Sinif = stajyer.Sinif ?? "",
                    StajTuru = stajyer.StajTuru ?? "",
                    StajBaslangicTarihi = stajyer.StajBaslangicTarihi ?? DateTime.Now,
                    StajBitisTarihi = stajyer.StajBitisTarihi ?? DateTime.Now.AddDays(60),
                    BasvuruTarihi = stajyer.BasvuruTarihi ?? stajyer.KayitTarihi ?? DateTime.Now,
                    CVDosyaYolu = stajyer.CVDosyaYolu,
                    FotografDosyaYolu = stajyer.FotografDosyaYolu,
                    BasvuruDurumu = stajyer.BasvuruDurumu ?? "",
                    IlerlemeYuzdesi = GetIlerlemeYuzdesi(stajyer.StajBaslangicTarihi, stajyer.StajBitisTarihi)
                };

                return View(detayModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Stajyer detayları alınırken hata oluştu.";
                Console.WriteLine($"StajyerDetay error: {ex.Message}");
                return RedirectToAction("StajyerListesi");
            }
        }
        // EgitmenController.cs dosyasına eklenecek methodlar
        
            
               
        public async Task<IActionResult> OdevTeslimleri(int gorevId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Belirli bir göreve ait teslimleri getir
            var gorev = await _context.Gorevler
                .FirstOrDefaultAsync(g => g.Id == gorevId && g.EgitmenId == userId.Value);

            if (gorev == null)
            {
                TempData["Error"] = "Görev bulunamadı!";
                return RedirectToAction("OdevAtama"); // veya ana sayfa
            }

            var teslimler = await _context.OdevTeslimler
                .Include(ot => ot.StajyerGorev)
                    .ThenInclude(sg => sg.User)
                .Where(ot => ot.StajyerGorev.GorevId == gorevId)
                .OrderByDescending(ot => ot.TeslimTarihi)
                .ToListAsync();

            ViewBag.GorevAdi = gorev.GorevAdi;
            ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Eğitmen";

            return View(teslimler);
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> OdevOnayla(int teslimId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            try
            {
                var teslim = await _context.OdevTeslimler
                    .Include(ot => ot.StajyerGorev)  // Bu satırı ekle
                    .FirstOrDefaultAsync(ot => ot.Id == teslimId);

                if (teslim == null)
                {
                    TempData["Error"] = "Teslim bulunamadı!";
                    return RedirectToAction("OdevAtama");
                }

                teslim.Durum = TeslimDurumu.Onaylandi;
                teslim.DegerlendirmeTarihi = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Ödev başarıyla onaylandı!";
                return RedirectToAction("OdevTeslimleri", new { gorevId = teslim.StajyerGorev.GorevId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Onaylama sırasında hata oluştu.";
                return RedirectToAction("OdevAtama");
            }
        }

        [HttpPost]
        public async Task<IActionResult> OdevReddet(int teslimId, string redNedeni)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrEmpty(redNedeni))
            {
                TempData["Error"] = "Red nedeni belirtmelisiniz!";
                return RedirectToAction("OdevTeslimleri", new { gorevId = 1 });
            }

            try
            {
                var teslim = await _context.OdevTeslimler
                    .Include(ot => ot.StajyerGorev)
                    .FirstOrDefaultAsync(ot => ot.Id == teslimId);

                if (teslim == null)
                {
                    TempData["Error"] = "Teslim bulunamadı!";
                    return RedirectToAction("OdevAtama");
                }

                teslim.Durum = TeslimDurumu.Reddedildi;
                teslim.DegerlendirmeTarihi = DateTime.Now;
                teslim.EgitmenGeriBildirimi = redNedeni;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Ödev reddedildi ve stajyere bildirim gönderildi.";
                return RedirectToAction("OdevTeslimleri", new { gorevId = teslim.StajyerGorev.GorevId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Reddetme sırasında hata oluştu.";
                return RedirectToAction("OdevAtama");
            }
        }
        // Eğitim Yönetimi ana sayfası
        public async Task<IActionResult> EgitimYonetimi()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Mevcut eğitimleri çek
            var egitimler = await _context.Egitimler
                .Include(e => e.Katilimlar)
                    .ThenInclude(k => k.Stajyer)
                .OrderByDescending(e => e.Tarih)
                .ToListAsync();

            return View(egitimler);
        }

        // Yeni eğitim oluşturma sayfası
        public async Task<IActionResult> YeniEgitim()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
            return View();
        }

        // Yeni eğitim oluşturma
        [HttpPost]
        public async Task<IActionResult> YeniEgitim(YeniEgitimViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
                return View(model);
            }

            if (model.SeciliStajyerler == null || !model.SeciliStajyerler.Any())
            {
                ModelState.AddModelError("SeciliStajyerler", "En az bir stajyer seçmelisiniz!");
                ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
                return View(model);
            }

            try
            {
                // Eğitim oluştur
                var yeniEgitim = new Egitim
                {
                    EgitimAdi = model.EgitimAdi,
                    Aciklama = model.Aciklama,
                    Tarih = model.Tarih.Date + model.Saat,
                    Saat = model.Saat
                };

                _context.Egitimler.Add(yeniEgitim);
                await _context.SaveChangesAsync();

                // Seçilen stajyerlere atama yap
                foreach (var stajyerId in model.SeciliStajyerler)
                {
                    var katilim = new EgitimKatilimi
                    {
                        EgitimId = yeniEgitim.Id,
                        StajyerId = stajyerId,
                        AtamaTarihi = DateTime.Now,
                        KatildiMi = null // Henüz karar vermedi
                    };
                    _context.EgitimKatilimlari.Add(katilim);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"'{model.EgitimAdi}' eğitimi oluşturuldu ve {model.SeciliStajyerler.Count} stajyere atandı!";
                return RedirectToAction("EgitimYonetimi");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Eğitim oluşturulurken hata: {ex.Message}";
                ViewBag.AktifStajyerler = await GetGercekAktifStajyerler();
                return View(model);
            }
        }
        // Duyuru listesi ve yeni duyuru formu
        public async Task<IActionResult> DuyuruYonetimi()
        {
            var egitmenId = HttpContext.Session.GetInt32("UserId");
            if (egitmenId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var duyurular = await _context.EgitmenDuyurulari
                    .Include(d => d.Egitmen)
                    .Where(d => d.EgitmenId == egitmenId.Value)
                    .OrderByDescending(d => d.OlusturmaTarihi)
                    .Take(20)
                    .ToListAsync();

                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                return View(duyurular);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Duyurular yüklenirken hata: {ex.Message}");
                TempData["Error"] = "Duyurular yüklenirken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // Yeni duyuru oluşturma
        [HttpPost]
        public async Task<IActionResult> DuyuruOlustur(string baslik, string mesaj, DuyuruHedefKitle hedefKitle)
        {
            var egitmenId = HttpContext.Session.GetInt32("UserId");
            if (egitmenId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrWhiteSpace(baslik) || string.IsNullOrWhiteSpace(mesaj))
            {
                TempData["Error"] = "Başlık ve mesaj alanları boş olamaz!";
                return RedirectToAction("DuyuruYonetimi");
            }

            try
            {
                var yeniDuyuru = new EgitmenDuyurusu
                {
                    Baslik = baslik.Trim(),
                    Mesaj = mesaj.Trim(),
                    EgitmenId = egitmenId.Value,
                    HedefKitle = hedefKitle,
                    OlusturmaTarihi = DateTime.Now,
                    Aktif = true
                };

                _context.EgitmenDuyurulari.Add(yeniDuyuru);
                await _context.SaveChangesAsync();

                await _context.SaveChangesAsync();

                // Her stajyer için bildirim oluştur
                var aktifStajyerler = await _context.Users
                    .Where(u => u.UserType == UserType.Stajyer && u.BasvuruDurumu == "Onaylandi")
                    .ToListAsync();

                foreach (var stajyer in aktifStajyerler)
                {
                    var bildirim = new SistemBildirimi
                    {
                        StajyerId = stajyer.Id,
                        Baslik = "Yeni Eğitmen Duyurusu",
                        Mesaj = $"{baslik}: {mesaj.Substring(0, Math.Min(50, mesaj.Length))}...",
                        Tur = BildirimTuru.EgitmenDuyurusu,
                        IlgiliId = yeniDuyuru.Id,
                        OlusturmaTarihi = DateTime.Now,
                        Okundu = false
                    };
                    _context.SistemBildirimleri.Add(bildirim);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ {aktifStajyerler.Count} stajyere duyuru bildirimi gönderildi");

                TempData["Success"] = "Duyuru başarıyla oluşturuldu!";

                TempData["Success"] = "Duyuru başarıyla oluşturuldu!";
                Console.WriteLine($"✅ Yeni duyuru oluşturuldu: {baslik}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Duyuru oluşturma hatası: {ex.Message}");
                TempData["Error"] = "Duyuru oluşturulurken hata oluştu.";
            }

            return RedirectToAction("DuyuruYonetimi");
        }

        public async Task<IActionResult> StajyerlerExcelAktar()
        {
            var egitmenId = HttpContext.Session.GetInt32("UserId");
            if (egitmenId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // Onaylı stajyerleri çek
                var stajyerler = await _context.Users
                    .Where(u => u.UserType == UserType.Stajyer && u.BasvuruDurumu == "Onaylandi")
                    .ToListAsync();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Stajyerler");

                    // Başlıkları ekle
                    worksheet.Cell(1, 1).Value = "Ad Soyad";
                    worksheet.Cell(1, 2).Value = "Email";
                    worksheet.Cell(1, 3).Value = "Telefon";
                    worksheet.Cell(1, 4).Value = "Okul";
                    worksheet.Cell(1, 5).Value = "Bölüm";
                    worksheet.Cell(1, 6).Value = "Sınıf";
                    worksheet.Cell(1, 7).Value = "Staj Türü";
                    worksheet.Cell(1, 8).Value = "Başlangıç Tarihi";
                    worksheet.Cell(1, 9).Value = "Bitiş Tarihi";
                    worksheet.Cell(1, 10).Value = "İlerleme";
                    worksheet.Cell(1, 11).Value = "Durum";

                    // Verileri ekle
                    for (int i = 0; i < stajyerler.Count; i++)
                    {
                        var stajyer = stajyerler[i];
                        var row = i + 2; // Başlık satırından sonra

                        worksheet.Cell(row, 1).Value = stajyer.AdSoyad ?? "";
                        worksheet.Cell(row, 2).Value = stajyer.Email ?? "";
                        worksheet.Cell(row, 3).Value = stajyer.Telefon ?? "";
                        worksheet.Cell(row, 4).Value = stajyer.OkulAdi ?? "";
                        worksheet.Cell(row, 5).Value = stajyer.Bolum ?? "";
                        worksheet.Cell(row, 6).Value = stajyer.Sinif ?? "";
                        worksheet.Cell(row, 7).Value = stajyer.StajTuru ?? "";
                        worksheet.Cell(row, 8).Value = stajyer.StajBaslangicTarihi?.ToString("dd/MM/yyyy") ?? "";
                        worksheet.Cell(row, 9).Value = stajyer.StajBitisTarihi?.ToString("dd/MM/yyyy") ?? "";
                        worksheet.Cell(row, 10).Value = GetIlerlemeMetni(stajyer.StajBaslangicTarihi, stajyer.StajBitisTarihi);
                        worksheet.Cell(row, 11).Value = stajyer.BasvuruDurumu ?? "";
                    }

                    // Başlık satırını stillendir
                    var headerRange = worksheet.Range(1, 1, 1, 11);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

                    // Sütun genişliklerini ayarla
                    worksheet.Columns().AdjustToContents();

                    var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    var fileName = $"Stajyerler_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                    Console.WriteLine($"Excel dosyası oluşturuldu: {fileName} - {stajyerler.Count} stajyer");

                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excel export hatası: {ex.Message}");
                TempData["Error"] = "Excel dosyası oluşturulurken hata oluştu.";
                return RedirectToAction("StajyerListesi");
            }
        }

    }
}