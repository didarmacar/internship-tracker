using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerTakipSistemi.Data;
using StajyerTakipSistemi.Models;
using StajyerTakipSistemi.Models.ViewModels;
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

            await MevcutAtamalariNotifyYap();
            await SetBildirimSayisi(userId.Value);

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
                var user = await _context.Users.FindAsync(userId.Value);
                var redNedeni = user?.BasvuruDurumu?.Replace("Reddedildi: ", "") ?? "Belirtilmemiş";
                ViewBag.RedNedeni = redNedeni;

                return View("BasvuruReddedildi");
            }
            else if (basvuruDurumu == "Onaylandi")
            {
                // ✅ GERÇEKÇİ DASHBOARD VERİLERİ
                string userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";

                var dashboardData = new StajyerDashboardViewModel
                {
                    KullaniciAdi = userName,
                    KullaniciInitials = GetInitials(userName),
                    BekleyenOdevler = await GetRealBekleyenOdevler(userId.Value),
                    BugunEgitimleri = await GetRealBugunEgitimleri(userId.Value),
                    GenelIlerleme = await CalculateGenelIlerleme(userId.Value),
                    BuHaftaIlerleme = await CalculateBuHaftaIlerleme(userId.Value),
                    SonAktiviteler = await GetRealSonAktiviteler(userId.Value)
                };

                return View("Index", dashboardData);
            }

            return View("BasvuruIndex");
        }

        // ✅ YENİ: Gerçek bekleyen ödevleri getir
        private async Task<List<OdevViewModel>> GetRealBekleyenOdevler(int userId)
        {
            try
            {
                var bekleyenOdevler = await _context.StajyerGorevler
                    .Include(sg => sg.Gorev)
                    .Where(sg => sg.UserId == userId && !sg.Tamamlandi)
                    .Where(sg => !_context.OdevTeslimler.Any(ot => ot.StajyerGorevId == sg.Id)) // Teslim edilmemiş olanlar
                    .Select(sg => new OdevViewModel
                    {
                        GorevAdi = sg.Gorev.GorevAdi,
                        Durum = DateTime.Now > sg.Gorev.TeslimTarihi ? "Gecikmiş" :
                               (sg.Gorev.TeslimTarihi - DateTime.Now).Days <= 1 ? "Acil" : "Bekliyor"
                    })
                    .Take(5)
                    .ToListAsync();

                // Eğer hiç bekleyen ödev yoksa
                if (!bekleyenOdevler.Any())
                {
                    bekleyenOdevler.Add(new OdevViewModel
                    {
                        GorevAdi = "Tebrikler! Tüm ödevlerin tamamlandı",
                        Durum = "✅"
                    });
                }

                return bekleyenOdevler;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Bekleyen ödevler yüklenirken hata: {ex.Message}");
                return new List<OdevViewModel>
        {
            new OdevViewModel { GorevAdi = "Veri yüklenemedi", Durum = "Hata" }
        };
            }
        }

        // ✅ YENİ: Bugünkü gerçek eğitimleri getir
        private async Task<List<EgitimViewModel>> GetRealBugunEgitimleri(int userId)
        {
            try
            {
                var bugun = DateTime.Today;
                var bugunEgitimleri = await _context.EgitimKatilimlari
                    .Include(ek => ek.Egitim)
                    .Where(ek => ek.StajyerId == userId &&
                               ek.Egitim.Tarih.Date == bugun)
                    .Select(ek => new EgitimViewModel
                    {
                        Saat = ek.Egitim.Saat.ToString(@"hh\:mm") + " - " +
                               ek.Egitim.Saat.Add(TimeSpan.FromHours(1)).ToString(@"hh\:mm"),
                        Baslik = ek.Egitim.EgitimAdi
                    })
                    .ToListAsync();

                // Eğer bugün eğitim yoksa yakın zamandaki eğitimleri göster
                if (!bugunEgitimleri.Any())
                {
                    var yakindakiEgitimler = await _context.EgitimKatilimlari
                        .Include(ek => ek.Egitim)
                        .Where(ek => ek.StajyerId == userId &&
                                   ek.Egitim.Tarih >= DateTime.Today)
                        .OrderBy(ek => ek.Egitim.Tarih)
                        .Take(2)
                        .Select(ek => new EgitimViewModel
                        {
                            Saat = ek.Egitim.Tarih.ToString("dd/MM"),
                            Baslik = ek.Egitim.EgitimAdi + " (Yakında)"
                        })
                        .ToListAsync();

                    return yakindakiEgitimler;
                }

                return bugunEgitimleri;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Bugün eğitimleri yüklenirken hata: {ex.Message}");
                return new List<EgitimViewModel>
                {
                    new EgitimViewModel { Saat = "N/A", Baslik = "Veri yüklenemedi" }
                };
            }
        }

        // ✅ YENİ: Genel ilerleme hesapla
        private async Task<int> CalculateGenelIlerleme(int userId)
        {
            try
            {
                var toplamGorevler = await _context.StajyerGorevler
                    .Where(sg => sg.UserId == userId)
                    .CountAsync();

                if (toplamGorevler == 0) return 0;

                var tamamlananGorevler = await _context.StajyerGorevler
                    .Where(sg => sg.UserId == userId && sg.Tamamlandi)
                    .CountAsync();

                return (int)Math.Round((double)tamamlananGorevler / toplamGorevler * 100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Genel ilerleme hesaplanırken hata: {ex.Message}");
                return 0;
            }
        }

        // ✅ YENİ: Bu hafta ilerleme hesapla
        private async Task<int> CalculateBuHaftaIlerleme(int userId)
        {
            try
            {
                var haftaBaslangici = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var haftaSonu = haftaBaslangici.AddDays(7);

                var buHaftaGorevler = await _context.StajyerGorevler
                    .Include(sg => sg.Gorev)
                    .Where(sg => sg.UserId == userId &&
                               sg.AtamaTarihi >= haftaBaslangici &&
                               sg.AtamaTarihi < haftaSonu)
                    .CountAsync();

                if (buHaftaGorevler == 0) return 100; // Hiç görev yoksa %100

                var buHaftaTamamlanan = await _context.StajyerGorevler
                    .Include(sg => sg.Gorev)
                    .Where(sg => sg.UserId == userId &&
                               sg.AtamaTarihi >= haftaBaslangici &&
                               sg.AtamaTarihi < haftaSonu &&
                               sg.Tamamlandi)
                    .CountAsync();

                return (int)Math.Round((double)buHaftaTamamlanan / buHaftaGorevler * 100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Bu hafta ilerleme hesaplanırken hata: {ex.Message}");
                return 0;
            }
        }

        // ✅ YENİ: Gerçek son aktiviteleri getir
        private async Task<List<AktiviteViewModel>> GetRealSonAktiviteler(int userId)
        {
            try
            {
                var aktiviteler = new List<AktiviteViewModel>();

                // Son teslim edilen ödevler
                var sonTeslimler = await _context.OdevTeslimler
                    .Include(ot => ot.StajyerGorev)
                        .ThenInclude(sg => sg.Gorev)
                    .Where(ot => ot.StajyerGorev.UserId == userId)
                    .OrderByDescending(ot => ot.TeslimTarihi)
                    .Take(2)
                    .ToListAsync();

                foreach (var teslim in sonTeslimler)
                {
                    aktiviteler.Add(new AktiviteViewModel
                    {
                        Icon = teslim.Durum == TeslimDurumu.Onaylandi ? "✅" : "📤",
                        IconClass = teslim.Durum == TeslimDurumu.Onaylandi ? "activity-completed" : "activity-progress",
                        Baslik = $"{teslim.StajyerGorev.Gorev.GorevAdi} teslim edildi",
                        Aciklama = teslim.Durum == TeslimDurumu.Onaylandi ?
                                 "Eğitmen tarafından onaylandı" :
                                 "Değerlendirme bekleniyor"
                    });
                }

                // Son katılınan eğitimler
                var sonEgitimler = await _context.EgitimKatilimlari
                    .Include(ek => ek.Egitim)
                    .Where(ek => ek.StajyerId == userId && ek.KatildiMi == true)
                    .OrderByDescending(ek => ek.KatilimTarihi)
                    .Take(2)
                    .ToListAsync();

                foreach (var egitim in sonEgitimler)
                {
                    aktiviteler.Add(new AktiviteViewModel
                    {
                        Icon = "🎓",
                        IconClass = "activity-completed",
                        Baslik = $"{egitim.Egitim.EgitimAdi} eğitimine katıldı",
                        Aciklama = egitim.KatilimTarihi?.ToString("dd/MM/yyyy") ?? ""
                    });
                }

                // Eğer aktivite yoksa varsayılan mesaj ekle
                if (!aktiviteler.Any())
                {
                    aktiviteler.Add(new AktiviteViewModel
                    {
                        Icon = "🚀",
                        IconClass = "activity-progress",
                        Baslik = "Staja hoş geldin!",
                        Aciklama = "İlk aktiviteleriniz burada görünecek"
                    });
                }

                return aktiviteler.Take(4).ToList(); // En fazla 4 aktivite göster
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Son aktiviteler yüklenirken hata: {ex.Message}");
                return new List<AktiviteViewModel>
                {
                    new AktiviteViewModel
                    {
                        Icon = "⚠️",
                        IconClass = "activity-progress",
                        Baslik = "Veri yüklenemedi",
                        Aciklama = "Aktiviteler şu anda görüntülenemiyor"
                    }
                };
            }
        }

        public async Task<IActionResult> Odevlerim()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"🔍 Session UserId: {userId}");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var basvuruDurumu = GetUserBasvuruDurumu(userId.Value);
            if (basvuruDurumu != "Onaylandi")
            {
                TempData["Error"] = "Bu sayfaya erişmek için başvurunuzun onaylanması gerekiyor.";
                return RedirectToAction("Index");
            }


            await SetBildirimSayisi(userId.Value);

            // DEBUG: Database'deki verileri kontrol et
            try
            {
                var tumUsers = await _context.Users.ToListAsync();
                Console.WriteLine($"📊 Toplam kullanıcı sayısı: {tumUsers.Count}");
                foreach (var user in tumUsers)
                {
                    Console.WriteLine($"   - {user.Id}: {user.AdSoyad} ({user.UserType})");
                }

                var tumGorevler = await _context.Gorevler.ToListAsync();
                Console.WriteLine($"📋 Toplam görev sayısı: {tumGorevler.Count}");
                foreach (var gorev in tumGorevler)
                {
                    Console.WriteLine($"   - {gorev.Id}: {gorev.GorevAdi}");
                }

                var tumAtamalar = await _context.StajyerGorevler.Include(sg => sg.User).Include(sg => sg.Gorev).ToListAsync();
                Console.WriteLine($"🎯 Toplam atama sayısı: {tumAtamalar.Count}");
                foreach (var atama in tumAtamalar)
                {
                    Console.WriteLine($"   - UserId: {atama.UserId}, Görev: {atama.Gorev?.GorevAdi}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database okuma hatası: {ex.Message}");
            }

            // ✅ GetStajyerOdevleri metodunu çağır - SADECE BİR KEZ!
            var odevler = await GetStajyerOdevleri(userId.Value);
            Console.WriteLine($"📝 Bulunan ödev sayısı: {odevler.Count}");

            ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";
            return View(odevler);
        }
        private async Task<List<StajyerOdevViewModel>> GetStajyerOdevleri(int userId)
        {
            Console.WriteLine($"🔍 GetStajyerOdevleri çağrıldı, UserId: {userId}");

            var stajyerGorevleri = await _context.StajyerGorevler
                .Include(sg => sg.Gorev)
                .Include(sg => sg.User)
                .Where(sg => sg.UserId == userId)
                .ToListAsync();

            Console.WriteLine($"📋 Bulunan stajyer görevleri: {stajyerGorevleri.Count}");

            var odevler = new List<StajyerOdevViewModel>();

            foreach (var sg in stajyerGorevleri)
            {
                Console.WriteLine($"🔄 İşleniyor: {sg.Gorev?.GorevAdi}");

                // Teslim durumunu kontrol et
                OdevTeslim? teslim = null;
                try
                {
                    teslim = await _context.OdevTeslimler
                        .Include(ot => ot.Mesajlar)
                            .ThenInclude(m => m.Gonderen)
                        .FirstOrDefaultAsync(ot => ot.StajyerGorevId == sg.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Teslim kontrolü hatası: {ex.Message}");
                }

                var kalanGun = (sg.Gorev.TeslimTarihi - DateTime.Now).Days;
                var geciktiMi = DateTime.Now > sg.Gorev.TeslimTarihi && !sg.Tamamlandi;

                var odev = new StajyerOdevViewModel
                {
                    StajyerGorevId = sg.Id,
                    GorevId = sg.GorevId,
                    GorevAdi = sg.Gorev.GorevAdi ?? "",
                    Aciklama = sg.Gorev.Aciklama ?? "",
                    AtamaTarihi = sg.AtamaTarihi,
                    TeslimTarihi = sg.Gorev.TeslimTarihi,
                    ZorlukSeviyesi = sg.Gorev.ZorlukSeviyesi ?? "",
                    Tamamlandi = sg.Tamamlandi,
                    TeslimEdildi = teslim != null,
                    GeciktiMi = geciktiMi,
                    KalanGun = kalanGun
                };

                // Teslim durumuna göre durum belirleme
                if (teslim != null)
                {
                    odev.Durum = teslim.Durum switch
                    {
                        TeslimDurumu.Beklemede => "Değerlendirme Bekliyor",
                        TeslimDurumu.Degerlendiriliyor => "Değerlendiriliyor",
                        TeslimDurumu.Onaylandi => "Onaylandı",
                        TeslimDurumu.Reddedildi => "Revize Gerekli",
                        TeslimDurumu.RevizeDeneme => "Revize Bekliyor",
                        _ => "Teslim Edildi"
                    };

                    odev.DurumClass = teslim.Durum switch
                    {
                        TeslimDurumu.Onaylandi => "text-success",
                        TeslimDurumu.Reddedildi => "text-danger",
                        TeslimDurumu.RevizeDeneme => "text-warning",
                        _ => "text-info"
                    };

                    // ✅ YENİ: Teslim detayını ekle
                    odev.TeslimDetay = new OdevTeslimDetayViewModel
                    {
                        Id = teslim.Id,
                        DosyaAdi = teslim.DosyaAdi,
                        DosyaYolu = teslim.DosyaYolu,
                        DosyaBoyutu = teslim.DosyaBoyutu,
                        TeslimTarihi = teslim.TeslimTarihi,
                        StajyerNotu = teslim.StajyerNotu,
                        Durum = teslim.Durum,
                        DegerlendirmeTarihi = teslim.DegerlendirmeTarihi,
                        EgitmenGeriBildirimi = teslim.EgitmenGeriBildirimi,
                        Puan = teslim.Puan,
                        Mesajlar = teslim.Mesajlar?.Select(m => new OdevMesajViewModel
                        {
                            Id = m.Id,
                            GonderenAdi = m.Gonderen?.AdSoyad ?? "Bilinmeyen",
                            Mesaj = m.Mesaj,
                            GonderimTarihi = m.GonderimTarihi,
                            BenimMesajim = m.GonderenId == userId,
                            EkDosyaYolu = m.EkDosyaYolu
                        }).OrderBy(m => m.GonderimTarihi).ToList() ?? new List<OdevMesajViewModel>()
                    };
                }
                else if (geciktiMi)
                {
                    odev.Durum = "Gecikmiş";
                    odev.DurumClass = "text-danger";
                }
                else
                {
                    odev.Durum = kalanGun > 0 ? $"{kalanGun} gün kaldı" : "Bugün teslim";
                    odev.DurumClass = kalanGun <= 1 ? "text-warning" : "text-primary";
                }

                odevler.Add(odev);
                Console.WriteLine($"✅ Ödev eklendi: {odev.GorevAdi} - Durum: {odev.Durum}");
            }

            Console.WriteLine($"📝 Toplam dönüştürülen ödev sayısı: {odevler.Count}");
            return odevler.OrderBy(o => o.TeslimTarihi).ToList();
        }

        [HttpPost]
        public async Task<IActionResult> OdevTeslimEt(OdevTeslimViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Lütfen gerekli alanları doldurun!";
                return RedirectToAction("Odevlerim");
            }

            // Dosya kontrolü
            if (model.OdevDosyasi == null || model.OdevDosyasi.Length == 0)
            {
                TempData["Error"] = "Lütfen bir dosya seçiniz!";
                return RedirectToAction("Odevlerim");
            }

            // Dosya tipini kontrol et (sadece PDF)
            if (!model.OdevDosyasi.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Sadece PDF dosyaları kabul edilmektedir!";
                return RedirectToAction("Odevlerim");
            }

            // Dosya boyutu kontrolü (10MB max)
            if (model.OdevDosyasi.Length > 10 * 1024 * 1024)
            {
                TempData["Error"] = "Dosya boyutu 10MB'dan büyük olamaz!";
                return RedirectToAction("Odevlerim");
            }


            try
            {
                // StajyerGorev kontrolü
                var stajyerGorev = await _context.StajyerGorevler
                    .Include(sg => sg.Gorev)
                    .Include(sg => sg.User)
                    .FirstOrDefaultAsync(sg => sg.Id == model.StajyerGorevId && sg.UserId == userId);

                if (stajyerGorev == null)
                {
                    TempData["Error"] = "Ödev bulunamadı!";
                    return RedirectToAction("Odevlerim");
                }

                // Daha önce teslim edilip edilmediğini kontrol et
                var mevcutTeslim = await _context.OdevTeslimler
                    .FirstOrDefaultAsync(ot => ot.StajyerGorevId == model.StajyerGorevId);

                if (mevcutTeslim != null && mevcutTeslim.Durum == TeslimDurumu.Onaylandi)
                {
                    TempData["Error"] = "Bu ödev zaten onaylanmış, yeniden teslim edilemez!";
                    return RedirectToAction("Odevlerim");
                }
                // Reddedilen ödev için yeniden teslim kontrolü
                if (mevcutTeslim != null && mevcutTeslim.Durum == TeslimDurumu.Reddedildi)
                {
                    TempData["Info"] = "Reddedilen ödevinizi yeniden gönderiyorsunuz.";
                }

                // Dosyayı kaydet
                var dosyaYolu = await SaveOdevFile(model.OdevDosyasi, stajyerGorev.Gorev.GorevAdi, stajyerGorev.User.AdSoyad);

                if (string.IsNullOrEmpty(dosyaYolu))
                {
                    TempData["Error"] = "Dosya kaydedilirken hata oluştu!";
                    return RedirectToAction("Odevlerim");
                }

                // Teslim kaydı oluştur veya güncelle
                if (mevcutTeslim != null)
                {
                    // Eski dosyayı sil
                    DeleteOldFile(mevcutTeslim.DosyaYolu);

                    // Mevcut teslimi güncelle
                    mevcutTeslim.DosyaYolu = dosyaYolu;
                    mevcutTeslim.DosyaAdi = model.OdevDosyasi.FileName;
                    mevcutTeslim.DosyaBoyutu = model.OdevDosyasi.Length;
                    mevcutTeslim.TeslimTarihi = DateTime.Now;
                    mevcutTeslim.StajyerNotu = model.StajyerNotu;
                    mevcutTeslim.Durum = mevcutTeslim.Durum == TeslimDurumu.Reddedildi ? TeslimDurumu.RevizeDeneme : TeslimDurumu.Beklemede;
                    mevcutTeslim.DegerlendirmeTarihi = null;
                    mevcutTeslim.EgitmenGeriBildirimi = null;
                    mevcutTeslim.Puan = null;
                }
                else
                {
                    // Yeni teslim oluştur
                    var yeniTeslim = new OdevTeslim
                    {
                        StajyerGorevId = model.StajyerGorevId,
                        DosyaYolu = dosyaYolu,
                        DosyaAdi = model.OdevDosyasi.FileName,
                        DosyaBoyutu = model.OdevDosyasi.Length,
                        TeslimTarihi = DateTime.Now,
                        StajyerNotu = model.StajyerNotu,
                        Durum = TeslimDurumu.Beklemede
                    };

                    _context.OdevTeslimler.Add(yeniTeslim);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Ödeviniz başarıyla teslim edildi! Eğitmeniniz değerlendirdikten sonra size geri bildirim verecektir.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ödev teslim hatası: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Odevlerim");
        }

        private async Task<string> SaveOdevFile(IFormFile file, string gorevAdi, string stajyerAdi)
        {
            try
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "odevler");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // Dosya adını güvenli hale getir
                var safeGorevAdi = string.Concat(gorevAdi.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))).Replace(" ", "_");
                var safeStajyerAdi = string.Concat(stajyerAdi.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))).Replace(" ", "_");

                var fileName = $"{safeGorevAdi}_{safeStajyerAdi}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/odevler/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dosya kaydetme hatası: {ex.Message}");
                return "";
            }
        }
        // Eğitimler sayfası (Takvim yerine)
        public async Task<IActionResult> Egitimler()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            await SetBildirimSayisi(userId.Value);

            // Başvuru durumu kontrolü
            var basvuruDurumu = GetUserBasvuruDurumu(userId.Value);
            if (basvuruDurumu != "Onaylandi")
            {
                return RedirectToAction("Index");
            }

            try
            {
                // Kullanıcıya atanan eğitimleri çek
                var egitimKatilimlari = await _context.EgitimKatilimlari
                    .Include(ek => ek.Egitim)
                    .Where(ek => ek.StajyerId == userId.Value)
                    .OrderByDescending(ek => ek.Egitim.Tarih)
                    .ToListAsync();

                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                return View(egitimKatilimlari);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eğitimler yüklenirken hata: {ex.Message}");
                TempData["Error"] = "Eğitimler yüklenirken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // Eğitime katıldım işareti
        [HttpPost]
        public async Task<IActionResult> EgitimaKatildim(int katilimId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var katilim = await _context.EgitimKatilimlari
                    .FirstOrDefaultAsync(ek => ek.Id == katilimId && ek.StajyerId == userId.Value);

                if (katilim == null)
                {
                    TempData["Error"] = "Eğitim bulunamadı!";
                    return RedirectToAction("Egitimler");
                }

                katilim.KatildiMi = true;
                katilim.KatilimTarihi = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Eğitime katılım kaydınız alındı!";
                return RedirectToAction("Egitimler");
            }
            catch (Exception )
            {
                TempData["Error"] = "İşlem sırasında hata oluştu.";
                return RedirectToAction("Egitimler");
            }
        }

        // Eğitime katılamadım işareti
        [HttpPost]
        public async Task<IActionResult> EgitimaKatilamadim(int katilimId, string neden)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var katilim = await _context.EgitimKatilimlari
                    .FirstOrDefaultAsync(ek => ek.Id == katilimId && ek.StajyerId == userId.Value);

                if (katilim == null)
                {
                    TempData["Error"] = "Eğitim bulunamadı!";
                    return RedirectToAction("Egitimler");
                }

                katilim.KatildiMi = false;
                katilim.KatilimTarihi = DateTime.Now;
                katilim.KatilmamaNedeni = neden;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Katılamama durumunuz kaydedildi.";
                return RedirectToAction("Egitimler");
            }
            catch (Exception )
            {
                TempData["Error"] = "İşlem sırasında hata oluştu.";
                return RedirectToAction("Egitimler");
            }
        }

        [HttpPost]
        public async Task<IActionResult> MesajGonder(YeniMesajViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Mesaj boş olamaz!";
                return RedirectToAction("Odevlerim");
            }

            try
            {
                // Ödev teslim kaydını kontrol et
                var odevTeslim = await _context.OdevTeslimler
                    .Include(ot => ot.StajyerGorev)
                    .FirstOrDefaultAsync(ot => ot.Id == model.OdevTeslimId
                        && ot.StajyerGorev.UserId == userId);

                if (odevTeslim == null)
                {
                    TempData["Error"] = "Ödev teslimi bulunamadı!";
                    return RedirectToAction("Odevlerim");
                }

                string? ekDosyaYolu = null;
                if (model.EkDosya != null && model.EkDosya.Length > 0)
                {
                    ekDosyaYolu = await SaveMessageFile(model.EkDosya);
                }

                var yeniMesaj = new OdevMesaj
                {
                    OdevTeslimId = model.OdevTeslimId,
                    GonderenId = userId.Value,
                    Mesaj = model.Mesaj,
                    GonderimTarihi = DateTime.Now,
                    EkDosyaYolu = ekDosyaYolu
                };

                _context.OdevMesajlar.Add(yeniMesaj);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Mesajınız gönderildi!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mesaj gönderme hatası: {ex.Message}");
                TempData["Error"] = "Mesaj gönderilirken hata oluştu!";
            }

            return RedirectToAction("Odevlerim");
        }

        private async Task<string> SaveMessageFile(IFormFile file)
        {
            try
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "mesajlar");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/mesajlar/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mesaj dosyası kaydetme hatası: {ex.Message}");
                return "";
            }
        }

        private void DeleteOldFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eski dosya silinemedi: {ex.Message}");
            }
        }


        public async Task<IActionResult> Mesajlar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            await SetBildirimSayisi(userId.Value);

            var basvuruDurumu = GetUserBasvuruDurumu(userId.Value);
            if (basvuruDurumu != "Onaylandi")
            {
                return RedirectToAction("Index");
            }

            try
            {
                // Sistem bildirimleri çek
                var sistemBildirimleri = await _context.SistemBildirimleri
                    .Where(sb => sb.StajyerId == userId.Value)
                    .OrderByDescending(sb => sb.OlusturmaTarihi)
                    .Take(10)
                    .ToListAsync();

                // Eğitmen duyurularını çek
                var egitmenDuyurulari = await _context.EgitmenDuyurulari
                    .Include(ed => ed.Egitmen)
                    .Where(ed => ed.Aktif && (ed.HedefKitle == DuyuruHedefKitle.TumStajyerler ||
                                              ed.HedefKitle == DuyuruHedefKitle.AktifStajyerler))
                    .OrderByDescending(ed => ed.OlusturmaTarihi)
                    .Take(10)
                    .ToListAsync();

                // Bildirimleri okundu olarak işaretle
                var okunmamisBildirimler = await _context.SistemBildirimleri
                    .Where(sb => sb.StajyerId == userId.Value && !sb.Okundu)
                    .ToListAsync();

                if (okunmamisBildirimler.Any())
                {
                    foreach (var bildirim in okunmamisBildirimler)
                    {
                        bildirim.Okundu = true;
                    }
                    await _context.SaveChangesAsync();
                }

                ViewBag.SistemBildirimleri = sistemBildirimleri;
                ViewBag.EgitmenDuyurulari = egitmenDuyurulari;
                ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hata: {ex.Message}");
                ViewBag.SistemBildirimleri = new List<SistemBildirimi>();
                ViewBag.EgitmenDuyurulari = new List<EgitmenDuyurusu>();
                ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";
                return View();
            }
        }

        // Zaman gösterimi helper metodu
        private string GetZamanGosterimi(DateTime tarih)
        {
            var fark = DateTime.Now - tarih;

            if (fark.TotalMinutes < 1) return "Az önce";
            if (fark.TotalMinutes < 60) return $"{(int)fark.TotalMinutes} dakika önce";
            if (fark.TotalHours < 24) return $"{(int)fark.TotalHours} saat önce";
            if (fark.TotalDays < 7) return $"{(int)fark.TotalDays} gün önce";

            return tarih.ToString("dd/MM/yyyy");
        }

        public async Task<IActionResult> KahveKutusu()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            await SetBildirimSayisi(userId.Value);
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

        public async Task<IActionResult> Basvuru()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            await SetBildirimSayisi(userId.Value);

            // Kullanıcının başvuru durumunu kontrol et
            var basvuruDurumu = GetUserBasvuruDurumu(userId.Value);

            if (basvuruDurumu != "Onaylandi")
            {
                return RedirectToAction("Index");
            }

            try
            {
                // Kullanıcının başvuru bilgilerini çek
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bilgileri bulunamadı!";
                    return RedirectToAction("Index");
                }

                // ViewModel'e dönüştür
                var basvuruModel = new StajyerBasvuruDetayViewModel
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
                    CVDosyaYolu = user.CVDosyaYolu,
                    FotografDosyaYolu = user.FotografDosyaYolu,
                    BasvuruTarihi = user.BasvuruTarihi ?? user.KayitTarihi ?? DateTime.Now,
                    BasvuruDurumu = user.BasvuruDurumu ?? ""
                };

                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                return View(basvuruModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Başvuru bilgileri çekme hatası: {ex.Message}");
                TempData["Error"] = "Başvuru bilgileri yüklenirken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> BasvuruGuncelle(StajyerBasvuruDetayViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Şifre değişikliği varsa özel validasyon
            if (!string.IsNullOrEmpty(model.YeniSifre))
            {
                if (string.IsNullOrEmpty(model.MevcutSifre))
                {
                    ModelState.AddModelError("MevcutSifre", "Yeni şifre belirlemek için mevcut şifrenizi girmelisiniz.");
                }

                if (string.IsNullOrEmpty(model.YeniSifreTekrar))
                {
                    ModelState.AddModelError("YeniSifreTekrar", "Yeni şifre tekrarı gereklidir.");
                }
                else if (model.YeniSifre != model.YeniSifreTekrar)
                {
                    ModelState.AddModelError("YeniSifreTekrar", "Şifreler eşleşmiyor.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                return View("Basvuru", model);
            }

            try
            {
                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı!";
                    return RedirectToAction("Basvuru");
                }

                // Şifre değişikliği kontrolü
                if (!string.IsNullOrEmpty(model.YeniSifre))
                {
                    // Mevcut şifreyi kontrol et
                    bool mevcutSifreDogruMu = false;
                    try
                    {
                        mevcutSifreDogruMu = BCrypt.Net.BCrypt.Verify(model.MevcutSifre, user.Sifre);
                    }
                    catch (Exception)
                    {
                        // BCrypt başarısız olursa düz metin kontrol et (geçici)
                        mevcutSifreDogruMu = (user.Sifre == model.MevcutSifre);
                    }

                    if (!mevcutSifreDogruMu)
                    {
                        ModelState.AddModelError("MevcutSifre", "Mevcut şifre yanlış!");
                        ViewBag.UserName = HttpContext.Session.GetString("UserName");
                        return View("Basvuru", model);
                    }

                    // Yeni şifreyi hashle ve kaydet
                    user.Sifre = BCrypt.Net.BCrypt.HashPassword(model.YeniSifre);
                    Console.WriteLine($"🔐 Şifre güncellendi: {user.AdSoyad}");
                }

                // Diğer bilgileri güncelle
                user.AdSoyad = model.AdSoyad;
                user.Telefon = model.Telefon;
                user.OkulAdi = model.OkulAdi;
                user.Bolum = model.Bolum;
                user.Sinif = model.Sinif;
                user.StajTuru = model.StajTuru;
                user.StajBaslangicTarihi = model.StajBaslangicTarihi;
                user.StajBitisTarihi = model.StajBitisTarihi;

                // Yeni dosyalar varsa işle
                if (model.YeniCVDosyasi != null)
                {
                    string cvPath = await SaveFile(model.YeniCVDosyasi, "cv");
                    if (!string.IsNullOrEmpty(cvPath))
                    {
                        // Eski CV'yi sil
                        if (!string.IsNullOrEmpty(user.CVDosyaYolu))
                        {
                            try
                            {
                                var oldCvPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.CVDosyaYolu.TrimStart('/'));
                                if (System.IO.File.Exists(oldCvPath))
                                {
                                    System.IO.File.Delete(oldCvPath);
                                    Console.WriteLine($"🗑️ Eski CV silindi: {user.CVDosyaYolu}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Eski CV silinemedi: {ex.Message}");
                            }
                        }
                        user.CVDosyaYolu = cvPath;
                    }
                }

                if (model.YeniFotografDosyasi != null)
                {
                    string fotoPath = await SaveFile(model.YeniFotografDosyasi, "foto");
                    if (!string.IsNullOrEmpty(fotoPath))
                    {
                        // Eski fotoğrafı sil
                        if (!string.IsNullOrEmpty(user.FotografDosyaYolu))
                        {
                            try
                            {
                                var oldFotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.FotografDosyaYolu.TrimStart('/'));
                                if (System.IO.File.Exists(oldFotoPath))
                                {
                                    System.IO.File.Delete(oldFotoPath);
                                    Console.WriteLine($"🗑️ Eski fotoğraf silindi: {user.FotografDosyaYolu}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Eski fotoğraf silinemedi: {ex.Message}");
                            }
                        }
                        user.FotografDosyaYolu = fotoPath;
                    }
                }

                await _context.SaveChangesAsync();

                // Başarı mesajı
                var successMessage = "Başvuru bilgileriniz başarıyla güncellendi!";
                if (!string.IsNullOrEmpty(model.YeniSifre))
                {
                    successMessage += " Şifreniz de güncellendi.";
                }

                TempData["Success"] = successMessage;
                Console.WriteLine($"✅ Stajyer başvuru bilgileri güncellendi: {model.AdSoyad} (ID: {userId})");

                return RedirectToAction("Basvuru");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 BAŞVURU GÜNCELLEME HATASI: {ex.Message}");
                TempData["Error"] = "Bilgiler güncellenirken hata oluştu.";
                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                return View("Basvuru", model);
            }
        }

        private string GetUserBasvuruDurumu(int userId)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null) return "Yapilmamis";

                // Başvuru durumuna göre döndür
                if (string.IsNullOrEmpty(user.BasvuruDurumu) || user.BasvuruDurumu == "Yapilmamis")
                {
                    return "Yapilmamis";
                }
                else if (user.BasvuruDurumu == "Beklemede")
                {
                    return "Beklemede";
                }
                else if (user.BasvuruDurumu.Contains("Reddedildi"))
                {
                    return "Reddedildi";
                }
                else if (user.BasvuruDurumu == "Onaylandi")
                {
                    return "Onaylandi";
                }

                return "Yapilmamis";
            }
            catch (Exception )
            {
                return "Yapilmamis";
            }
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
            catch (Exception )
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
                user.Sinif = model.Sinif ?? string.Empty;
                user.CVDosyaYolu = cvPath;
                user.FotografDosyaYolu = fotoPath;
                user.BasvuruDurumu = "Beklemede";
                user.BasvuruTarihi = DateTime.Now;

                await _context.SaveChangesAsync();
            }
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

        private async Task SetBildirimSayisi(int userId)
        {
            try
            {
                var okunmamisSayisi = await _context.SistemBildirimleri
                    .Where(sb => sb.StajyerId == userId && !sb.Okundu)
                    .CountAsync();

                ViewBag.OkunmamisBildirimSayisi = okunmamisSayisi;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Bildirim sayısı hesaplanırken hata: {ex.Message}");
                ViewBag.OkunmamisBildirimSayisi = 0;
            }
        }

        // Mevcut atamaları bildirime çevir
        private async Task MevcutAtamalariNotifyYap()
        {
            try
            {
                // Mevcut ödev atamaları için bildirim oluştur
                var mevcutOdevAtamalari = await _context.StajyerGorevler
                    .Include(sg => sg.Gorev)
                    .Include(sg => sg.User)
                    .Where(sg => !_context.SistemBildirimleri.Any(sb =>
                        sb.IlgiliId == sg.Id && sb.Tur == BildirimTuru.OdevAtamasi))
                    .ToListAsync();

                foreach (var atama in mevcutOdevAtamalari)
                {
                    var bildirim = new SistemBildirimi
                    {
                        StajyerId = atama.UserId,
                        Baslik = "Yeni Ödev Atandı",
                        Mesaj = $"{atama.Gorev.GorevAdi} ödeviniz atanmıştır. Teslim tarihi: {atama.Gorev.TeslimTarihi:dd/MM/yyyy}",
                        Tur = BildirimTuru.OdevAtamasi,
                        IlgiliId = atama.Id,
                        OlusturmaTarihi = atama.AtamaTarihi,
                        Okundu = false
                    };
                    _context.SistemBildirimleri.Add(bildirim);
                }

                // Mevcut eğitim atamaları için bildirim oluştur
                var mevcutEgitimAtamalari = await _context.EgitimKatilimlari
                    .Include(ek => ek.Egitim)
                    .Where(ek => !_context.SistemBildirimleri.Any(sb =>
                        sb.IlgiliId == ek.Id && sb.Tur == BildirimTuru.EgitimAtamasi))
                    .ToListAsync();

                foreach (var katilim in mevcutEgitimAtamalari)
                {
                    var bildirim = new SistemBildirimi
                    {
                        StajyerId = katilim.StajyerId,
                        Baslik = "Yeni Eğitime Kaydoldunuz",
                        Mesaj = $"{katilim.Egitim.EgitimAdi} eğitimine kaydoldunuz. Tarih: {katilim.Egitim.Tarih:dd/MM/yyyy} {katilim.Egitim.Saat:hh\\:mm}",
                        Tur = BildirimTuru.EgitimAtamasi,
                        IlgiliId = katilim.Id,
                        OlusturmaTarihi = katilim.AtamaTarihi,
                        Okundu = false
                    };
                    _context.SistemBildirimleri.Add(bildirim);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Mevcut atamalar için bildirimler oluşturuldu");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Mevcut atamaları bildirime çevirirken hata: {ex.Message}");
            }
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