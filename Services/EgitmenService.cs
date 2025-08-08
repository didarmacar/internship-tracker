using Microsoft.EntityFrameworkCore;
using StajyerTakipSistemi.Data;
using StajyerTakipSistemi.Models;
using StajyerTakipSistemi.Models.ViewModels;

namespace StajyerTakipSistemi.Services
{
    public class EgitmenService : IEgitmenService
    {
        private readonly StajyerTakipDbContext _context;

        public EgitmenService(StajyerTakipDbContext context)
        {
            _context = context;
        }

        public async Task<List<BasvuruDegerlendirmeViewModel>> GetBekleyenBasvurular()
        {
            try
            {
                var bekleyenUsers = await _context.Users
                    .Where(u => u.UserType == UserType.Stajyer && 
                               (u.BasvuruDurumu == "Beklemede" || u.BasvuruDurumu.Contains("Bekle")))
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
            catch
            {
                return new List<BasvuruDegerlendirmeViewModel>();
            }
        }

        public async Task<List<StajyerListeViewModel>> GetOnayliStajyerler()
        {
            try
            {
                var stajyerlar = await _context.Users
                    .Where(u => u.UserType == UserType.Stajyer && 
                               (u.BasvuruDurumu == "Onaylandi" || u.BasvuruDurumu.Contains("Onay")))
                    .ToListAsync();

                var result = new List<StajyerListeViewModel>();

                foreach (var user in stajyerlar)
                {
                    // Get related Stajyer entity for more detailed info
                    var stajyer = await _context.Stajyerler
                        .FirstOrDefaultAsync(s => s.UserId == user.Id);

                    // Calculate progress
                    var toplamGorev = await _context.StajyerGorevler
                        .CountAsync(sg => stajyer != null && sg.StajyerId == stajyer.Id);
                    var tamamlananGorev = await _context.StajyerGorevler
                        .CountAsync(sg => stajyer != null && sg.StajyerId == stajyer.Id && sg.Tamamlandi);

                    var ilerleme = toplamGorev > 0 ? (int)Math.Round((double)tamamlananGorev / toplamGorev * 100) : 0;

                    result.Add(new StajyerListeViewModel
                    {
                        Id = user.Id,
                        AdSoyad = user.AdSoyad ?? "",
                        Email = user.Email ?? "",
                        Telefon = user.Telefon ?? "",
                        OkulAdi = user.OkulAdi ?? "",
                        Bolum = user.Bolum ?? "",
                        StajTuru = user.StajTuru ?? "",
                        BaslangicTarihi = user.StajBaslangicTarihi ?? DateTime.Now,
                        BitisTarihi = user.StajBitisTarihi ?? DateTime.Now.AddDays(60),
                        Durum = GetStajyerDurum(stajyer),
                        IlerlemeYuzdesi = ilerleme
                    });
                }

                return result;
            }
            catch
            {
                return new List<StajyerListeViewModel>();
            }
        }

        public async Task<BasvuruDegerlendirmeViewModel?> GetBasvuruDetay(int userId)
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
            catch
            {
                return null;
            }
        }

        public async Task<bool> BasvuruOnayla(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.BasvuruDurumu = "Onaylandi";

                // Also update Stajyer entity if exists
                var stajyer = await _context.Stajyerler.FirstOrDefaultAsync(s => s.UserId == userId);
                if (stajyer != null)
                {
                    stajyer.Durum = StajDurumu.Aktif;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BasvuruReddet(int userId, string redNedeni)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.BasvuruDurumu = "Reddedildi";

                // Also update Stajyer entity if exists
                var stajyer = await _context.Stajyerler.FirstOrDefaultAsync(s => s.UserId == userId);
                if (stajyer != null)
                {
                    stajyer.Durum = StajDurumu.Reddedilmis;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> GorevAta(int gorevId, List<int> stajyerIds)
        {
            try
            {
                foreach (var stajyerId in stajyerIds)
                {
                    // Check if assignment already exists
                    var mevcutAtama = await _context.StajyerGorevler
                        .FirstOrDefaultAsync(sg => sg.GorevId == gorevId && sg.StajyerId == stajyerId);

                    if (mevcutAtama == null)
                    {
                        var yeniAtama = new StajyerGorev
                        {
                            GorevId = gorevId,
                            StajyerId = stajyerId,
                            AtamaTarihi = DateTime.Now,
                            Tamamlandi = false
                        };

                        _context.StajyerGorevler.Add(yeniAtama);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Gorev>> GetTumGorevler()
        {
            try
            {
                return await _context.Gorevler
                    .Include(g => g.Egitmen)
                    .Include(g => g.StajyerGorevler)
                    .OrderByDescending(g => g.OlusturmaTarihi)
                    .ToListAsync();
            }
            catch
            {
                return new List<Gorev>();
            }
        }

        public async Task<bool> YeniGorevOlustur(string gorevAdi, string aciklama, DateTime teslimTarihi, string zorlukSeviyesi, int egitmenId)
        {
            try
            {
                var yeniGorev = new Gorev
                {
                    GorevAdi = gorevAdi,
                    Aciklama = aciklama,
                    TeslimTarihi = teslimTarihi,
                    ZorlukSeviyesi = zorlukSeviyesi,
                    EgitmenId = egitmenId,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.Gorevler.Add(yeniGorev);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Helper methods
        private string GetBasvuruNotu(string? bolum)
        {
            return bolum switch
            {
                "Bilgisayar Mühendisliği" => "Yazılım geliştirme alanında deneyim kazanmak istiyor.",
                "Elektrik Mühendisliği" => "Elektronik ve kontrol sistemleri alanında çalışmak istiyor.",
                "Endüstri Mühendisliği" => "Süreç optimizasyonu ve proje yönetimi alanında gelişmek istiyor.",
                _ => "Staj sürecinde aktif katılım gösterecek adaydır."
            };
        }

        private string GetStajyerDurum(Stajyer? stajyer)
        {
            if (stajyer == null) return "Aktif";

            return stajyer.Durum switch
            {
                StajDurumu.OnayBekleyen => "Onay Bekliyor",
                StajDurumu.Aktif => "Aktif",
                StajDurumu.Reddedilmis => "Reddedilmiş",
                StajDurumu.Tamamlandi => "Tamamlandı",
                _ => "Aktif"
            };
        }
    }
}