using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerTakipSistemi.Models;
using StajyerTakipSistemi.Models.ViewModels;
using StajyerTakipSistemi.Data;

namespace StajyerTakipSistemi.Controllers
{
    public class AuthController : Controller
    {
        private readonly StajyerTakipDbContext _context;

        public AuthController(StajyerTakipDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            // DEBUG: Kullanıcı bulundu mu?
            if (user == null)
            {
                Console.WriteLine($"HATA: {model.Email} bulunamadı!");
                ModelState.AddModelError("", $"Kullanıcı bulunamadı: {model.Email}");
                return View(model);
            }

            // DEBUG: Şifre kontrolü
            Console.WriteLine($"User found: {user.Email}, UserType: {user.UserType}");
            Console.WriteLine($"Input password: {model.Sifre}");
            Console.WriteLine($"Stored password length: {user.Sifre?.Length}");

            bool passwordMatch = false;

            // Önce BCrypt ile dene
            try
            {
                passwordMatch = BCrypt.Net.BCrypt.Verify(model.Sifre, user.Sifre);
                Console.WriteLine($"BCrypt verification result: {passwordMatch}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BCrypt error: {ex.Message}");
                // Eğer BCrypt başarısız olursa, düz metin karşılaştır (geçici)
                passwordMatch = (user.Sifre == model.Sifre);
                Console.WriteLine($"Plain text comparison result: {passwordMatch}");
            }

            if (passwordMatch)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.AdSoyad ?? "");
                HttpContext.Session.SetInt32("UserType", (int)user.UserType);

                Console.WriteLine($"Login successful - UserType: {user.UserType}, Value: {(int)user.UserType}");

                if (user.UserType == UserType.Mentor) // 0
                {
                    Console.WriteLine("Redirecting to Egitmen/Index");
                    return RedirectToAction("Index", "Egitmen");
                }
                else // UserType.Stajyer (1)
                {
                    Console.WriteLine("Redirecting to Stajyer/Index");
                    return RedirectToAction("Index", "Stajyer");
                }
            }

            Console.WriteLine("Password verification failed!");
            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı");
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Email benzersizlik kontrolü
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor");
                return View(model);
            }

            try
            {
                // Password hashing with BCrypt
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Sifre);
                Console.WriteLine($"Registering user: {model.Email}, HashedPassword length: {hashedPassword.Length}");

                var user = new User
                {
                    AdSoyad = model.AdSoyad,
                    Email = model.Email,
                    Sifre = hashedPassword,
                    UserType = model.UserType,
                    KayitTarihi = DateTime.Now,
                    BasvuruDurumu = model.UserType == UserType.Stajyer ? null : "Aktif"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Kayıt başarıyla oluşturuldu! Şimdi giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                ModelState.AddModelError("", "Kayıt sırasında hata oluştu.");
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}