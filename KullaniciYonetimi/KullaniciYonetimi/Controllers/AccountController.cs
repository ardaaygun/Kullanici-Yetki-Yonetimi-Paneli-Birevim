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
            {
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
                        // 3. ADIM: Yaka Kartı (Claim) Oluşturma
                        // Sistemin kullanıcının kim olduğunu bilmesi için bilgileri bir listeye koyuyoruz.
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kimlik No
                            new Claim(ClaimTypes.Name, user.FirstName),               // Adı
                            new Claim(ClaimTypes.Email, user.Email),                  // E-postası
                            new Claim(ClaimTypes.Role, "Admin") // Şimdilik test için statik veriyoruz, sonra veritabanından çekeceğiz
                        };

                        // 4. ADIM: Tarayıcıya Çerez (Cookie) Bırakma İşlemi
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            // İstersen buraya "Beni Hatırla" (IsPersistent = true) özelliği de eklenebilir
                        };

                        // Giriş işlemini (SignIn) asenkron olarak tamamlıyoruz
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        // Her şey başarılıysa kullanıcıyı Ana Sayfaya yönlendir
                        return RedirectToAction("Index", "Home");
                    }
                }

                // Eğer e-posta yoksa veya şifre yanlışsa, güvenlik gereği hangisinin yanlış olduğunu 
                // açıkça söylemeyiz, genel bir hata veririz.
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            }

            return View(model);
        }
    }




}