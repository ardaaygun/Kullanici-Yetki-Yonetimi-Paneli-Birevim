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
        
        private readonly AppDbContext _context;

        // (Dependency Injection)
        
        public AccountController(AppDbContext context)
        {
            _context = context; // Gelen köprüyü, kendi değişkenimize eşitliyoruz ki içeride kullanabilelim
        }

        
        [HttpGet]
        public IActionResult Register()
        {
            return View(); 
        }

       
        
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            
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

                    // hash şifreleme
                    PasswordHash = BasitSifrelemeYap(model.Password),

                    
                    RoleId = 1
                };

                // KURAL 3: Veritabanına Kaydetme
                _context.Users.Add(newUser);
                _context.SaveChanges();      

                // Kayıt başarılıysa adamı Giriş Yap (Login) sayfasına yönlendiriyoruz
                return RedirectToAction("Login");
            }

            
            return View(model);
        }

        // Geçici bir şifre şifreleme algoritması (Hashleme)
        private string BasitSifrelemeYap(string plainText)
        {
            // Gelen düz metni (Base64) formatına çevirir. 
            
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }




        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null)
                {
                    // 2. ADIM: Şifre Kontrolü 
                    // (Kullanıcının girdiği şifreyi aynı yöntemle şifreleyip veritabanındaki ile karşılaştırıyoruz)
                    var hashedGirisSifresi = BasitSifrelemeYap(model.Password);

                    if (user.PasswordHash == hashedGirisSifresi)
                    {
                        // 1. Kullanıcının RoleId'sine bakarak Roles tablosundan rolün adını çekiyoruz
                        var userRole = _context.Roles.FirstOrDefault(r => r.Id == user.RoleId);

                        // Eğer bir terslik olur da rol bulunamazsa, güvenlik amacıyla varsayılan olarak "Standart" atıyoruz
                        string roleName = userRole != null ? userRole.RoleName : "Standart";

                        // 2. Yaka Kartı (Claim) Oluşturma
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.FirstName),
                            new Claim(ClaimTypes.Email, user.Email),                  
                            
                            
                            new Claim(ClaimTypes.Role, roleName)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties { };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        // --- ROL ID BAZLI YÖNLENDİRME MANTIĞI ---
                        if (user.RoleId == 3) // admin 
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (user.RoleId == 2) // (yönetici)
                        {
                            return RedirectToAction("Dashboard", "Home");
                        }
                        else // ( RoleId == 1)
                        {
                            return RedirectToAction("Dashboard", "Home");
                        }
                    }
                }

                
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            }

            return View(model);
        }

        
        public async Task<IActionResult> Logout()
        {
            // Tarayıcıdaki cookie siliyoruz . 
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            
            return RedirectToAction("Login", "Account");
        }
    }




}