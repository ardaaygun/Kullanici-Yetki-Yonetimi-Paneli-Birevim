using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KullaniciYonetimi.Models;
using KullaniciYonetimi.ViewModels;
using System.Linq;

namespace KullaniciYonetimi.Controllers
{
    // Authorize kimlik doğrulama ile erişim kısıtlandı.
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller 

    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Tüm Kullanıcıları Listeleme Sayfası
        public IActionResult Index()
        {
            // Veritabanındaki kullanıcıları alıp, arayüze göndereceğimiz ViewModel'e  çeviriyoruz
            var kullaniciListesi = _context.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email,
                RoleName = _context.Roles.FirstOrDefault(r => r.Id == u.RoleId).RoleName
            }).ToList();

            return View(kullaniciListesi);
        }

        // POST: Kullanıcının rolünü güncelleme işlemi
        // Bu metoda sadece formdan veri gönderildiğinde (Post) ulaşılabilir.
        [HttpPost]
        public IActionResult YetkiDegistir(int kullaniciId, int yeniRolId)
        {
            // 1. Veritabanından ID'si gelen kullanıcıyı bul
            var user = _context.Users.FirstOrDefault(u => u.Id == kullaniciId);

            if (user != null)
            {
                // 2. Kullanıcının mevcut rolünü yeni seçilen rolle değiştir
                user.RoleId = yeniRolId;

                // 3. Değişiklikleri SQL'e kaydet
                _context.SaveChanges();
            }

            
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

           
            return RedirectToAction("Index");
        }
    }

}