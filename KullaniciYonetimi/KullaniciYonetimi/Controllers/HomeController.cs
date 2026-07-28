using KullaniciYonetimi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace KullaniciYonetimi.Controllers
{
    public class HomeController : Controller
    {
        // KİLİTSİZ (Herkese Açık): Başında [Authorize] yok.
        // Siteye giren herkes (ziyaretçiler dahil) bu sayfayı görebilir.
        public IActionResult Index()
        {
            return View();
        }

        // 1. SEVİYE KİLİT: Sadece sisteme giriş yapmış olanlar görebilir.
        // Kimliği olmayan biri buraya girmeye çalışırsa, sistem onu otomatik olarak 
        // Program.cs'de belirlediğimiz "/Account/Login" adresine postalar.
        [Authorize]
        public IActionResult Profilim()
        {
            return View();
        }

        // 2. SEVİYE KİLİT (Role-Based): Sadece giriş yapmış VE rolü "Admin" olanlar görebilir.
        // Normal bir kullanıcı (User) buraya girmeye çalışırsa, giriş yapmış olsa bile 
        // yetkisi yetmediği için "/Account/AccessDenied" (Erişim Engellendi) sayfasına yönlendirilir.
        [Authorize(Roles = "Admin")]
        public IActionResult YonetimPaneli()
        {
            return View();
        }
    }
}
