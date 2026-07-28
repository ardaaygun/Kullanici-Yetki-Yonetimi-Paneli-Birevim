using Microsoft.AspNetCore.Mvc;
using KullaniciYonetimi.Models;
using KullaniciYonetimi.ViewModels;
using System.Linq;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KullaniciYonetimi.Controllers
{
    public class AccountController : Controller
    {
        // Veritabanı köprümüzü tutacağımız gizli (private) değişken
        private readonly AppDbContext _context;

        // 1. BAĞIMLILIK ENJEKSİYONU (Dependency Injection)
        // Controller ilk çalıştığında, Program.cs içindeki alet çantasından AppDbContext'i ister
        public AccountController(AppDbContext context)
        {
            _context = context; // Gelen köprüyü, kendi değişkenimize eşitliyoruz ki içeride kullanabilelim
        }

        // 2. GET: Arayüzü Ekrana Getirme İşlemi
        // Kullanıcı tarayıcıya "/Account/Register" yazdığında sadece boş formu görmek ister.
        [HttpGet]
        public IActionResult Register()
        {
            return View(); // Bu, "Git bana Register.cshtml arayüzünü bul ve ekrana bas" demektir.
        }

        // 3. POST: Formdan Gelen Veriyi Yakalama ve Kaydetme İşlemi
        // Kullanıcı "Kayıt Ol" butonuna bastığında veriler buraya düşer.
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            // ModelState.IsValid: Daha önce ViewModel'e yazdığımız [Required], [EmailAddress] kurallarına uyulmuş mu?
            if (ModelState.IsValid)
            {// KURAL 1: Birevim şirket maili zorunluluğu!
                // (OrdinalIgnoreCase: Kullanıcı @BIREVIM.COM.TR yazsa bile büyük/küçük harf takıntısı yapmadan kabul eder)
                if (!model.Email.EndsWith("@birevim.com.tr", System.StringComparison.OrdinalIgnoreCase))
                {
                    // Şart sağlanmıyorsa ekrana hata mesajı bas ve formu geri gönder
                    ModelState.AddModelError("Email", "Güvenlik İhlali: Sisteme sadece @birevim.com.tr uzantılı şirket e-postaları ile kayıt olunabilir!");
                    return View(model);
                }
                // KURAL 1: Aynı e-posta ile başka biri var mı?
                // Veritabanına (_context) gidip Users tablosunda bu e-postayı arıyoruz.
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    // Eğer bulursak, "Email" alanına özel bir hata mesajı ekleyip formu geri gönderiyoruz.
                    ModelState.AddModelError("Email", "Bu e-posta adresi sistemde zaten kayıtlı.");
                    return View(model);
                }

                // KURAL 2: ViewModel'i, Asıl Model'e (User) Çevirmek (Mapping)
                var newUser = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    // ŞİFRELEME: Şifreler veritabanına asla "123456" gibi düz yazılmaz!
                    PasswordHash = BasitSifrelemeYap(model.Password),

                    // İlişki gereği zorunlu: Şimdilik manuel olarak bir Rol ID atıyoruz 
                    // (Gerçek senaryoda bu rolu veritabanından dinamik çekeriz)
                    RoleId = 1
                };

                // KURAL 3: Veritabanına Kaydetme
                _context.Users.Add(newUser); // Tabloya satırı ekle
                _context.SaveChanges();      // Değişiklikleri SQL'e yansıt!

                // Kayıt başarılıysa adamı Giriş Yap (Login) sayfasına yönlendiriyoruz
                return RedirectToAction("Login");
            }

            // Eğer kurallara uyulmamışsa (Örn: şifreler eşleşmediyse), kullanıcının girdiği 
            // verileri (model) kaybetmeden aynı formu hatalarla birlikte geri göster.
            return View(model);
        }

        // Geçici bir şifre şifreleme algoritması (Hashleme)
        private string BasitSifrelemeYap(string plainText)
        {
            // Gelen düz metni anlaşılmaz bir formata (Base64) çevirir. 
            // (Kurumsal projelerde SHA256 veya BCrypt gibi daha güçlü algoritmalar kullanılır)
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }




        // GET: Sadece Login formunu ekrana getirir
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Kullanıcı e-posta ve şifresini gönderdiğinde çalışır
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. ADIM: Veritabanında bu e-postaya sahip bir kullanıcı var mı?
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null)
                {
                    // 2. ADIM: Şifre Kontrolü 
                    // (Kullanıcının girdiği şifreyi aynı yöntemle şifreleyip veritabanındaki ile karşılaştırıyoruz)
                    var hashedGirisSifresi = BasitSifrelemeYap(model.Password);

                    if (user.PasswordHash == hashedGirisSifresi)
                    {
                        // 1. YENİ EKLENEN KOD: Kullanıcının RoleId'sine bakarak Roles tablosundan rolün adını çekiyoruz
                        var userRole = _context.Roles.FirstOrDefault(r => r.Id == user.RoleId);

                        // Eğer bir terslik olur da rol bulunamazsa, güvenlik amacıyla varsayılan olarak "Standart" atıyoruz
                        string roleName = userRole != null ? userRole.RoleName : "Standart";

                        // 2. Yaka Kartı (Claim) Oluşturma
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.FirstName),
                            new Claim(ClaimTypes.Email, user.Email),                  
                            
                            // 3. GÜNCELLEME: "Admin" yazısını sildik, veritabanından gelen dinamik roleName değişkenini koyduk!
                            new Claim(ClaimTypes.Role, roleName)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties { };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        // --- ROL ID BAZLI YÖNLENDİRME MANTIĞI ---
                        if (user.RoleId == 3) // Varsayalım ki 3 numaralı ID "Admin" rolüne ait
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (user.RoleId == 2) // 2 numaralı ID "Yönetici" rolüne ait
                        {
                            return RedirectToAction("Dashboard", "Home");
                        }
                        else // Diğer standart kullanıcılar (Örn: RoleId == 1)
                        {
                            return RedirectToAction("Dashboard", "Home");
                        }
                    }
                }

                // Eğer e-posta yoksa veya şifre yanlışsa, güvenlik gereği hangisinin yanlış olduğunu 
                // açıkça söylemeyiz, genel bir hata veririz.
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            }

            return View(model);
        }

        // Çıkış yapma işlemi
        public async Task<IActionResult> Logout()
        {
            // Tarayıcıdaki çerezi (yaka kartını) siliyoruz
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Çıkış yaptıktan sonra kullanıcıyı Login sayfasına yönlendiriyoruz
            return RedirectToAction("Login", "Account");
        }
    }




}