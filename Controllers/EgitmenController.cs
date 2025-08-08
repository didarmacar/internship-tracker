using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerTakipSistemi.Models.ViewModels;
using StajyerTakipSistemi.Models;
using StajyerTakipSistemi.Data;
using StajyerTakipSistemi.Services;

namespace StajyerTakipSistemi.Controllers

{
    public class EgitmenController : Controller
    {
        private readonly StajyerTakipDbContext _context;
        private readonly IEgitmenService _egitmenService;

        public EgitmenController(StajyerTakipDbContext context, IEgitmenService egitmenService)
        {
            _context = context;
            _egitmenService = egitmenService;
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

            // Using service to get approved interns
            var stajyerler = await _egitmenService.GetOnayliStajyerler();

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

            // Using service to get pending applications
            var bekleyenBasvurular = await _egitmenService.GetBekleyenBasvurular();

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

            var success = await _egitmenService.BasvuruOnayla(userId);
            
            if (success)
            {
                TempData["Success"] = "Başvuru başarıyla onaylandı!";
            }
            else
            {
                TempData["Error"] = "Başvuru onaylanırken bir hata oluştu.";
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

            var success = await _egitmenService.BasvuruReddet(userId, redNedeni);
            
            if (success)
            {
                TempData["Success"] = "Başvuru başarıyla reddedildi.";
            }
            else
            {
                TempData["Error"] = "Başvuru reddedilirken bir hata oluştu.";
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

            // Using service to get application details
            var basvuru = await _egitmenService.GetBasvuruDetay(id);

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

            // Using service to get tasks
            var mevcutOdevler = await _egitmenService.GetTumGorevler();

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

            // Active interns list from service
            ViewBag.AktifStajyerler = await _egitmenService.GetOnayliStajyerler();
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
                ViewBag.AktifStajyerler = await _egitmenService.GetOnayliStajyerler();
                return View(model);
            }

            var egitmenId = HttpContext.Session.GetInt32("UserId").Value;
            
            // Create task using service
            var success = await _egitmenService.YeniGorevOlustur(
                model.GorevAdi, 
                model.Aciklama, 
                model.TeslimTarihi, 
                model.ZorlukSeviyesi, 
                egitmenId
            );

            if (success)
            {
                TempData["Success"] = $"'{model.GorevAdi}' ödevi başarıyla oluşturuldu!";
                return RedirectToAction("OdevAtama");
            }
            else
            {
                TempData["Error"] = "Ödev oluşturulurken bir hata oluştu.";
                ViewBag.AktifStajyerler = await _egitmenService.GetOnayliStajyerler();
                return View(model);
            }
        }
    }
}
