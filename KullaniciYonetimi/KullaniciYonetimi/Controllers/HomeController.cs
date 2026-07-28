using KullaniciYonetimi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace KullaniciYonetimi.Controllers
{
    public class HomeController : Controller
    {
        // 1. ANA SAYFA (Akýllý Yönlendirme)
        // Siteye ilk girildiðinde veya logoya týklandýðýnda çalýþýr.
        public IActionResult Index()
        {
            // Kullanýcý sisteme giriþ yapmýþ mý?
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Giriþ yapan kiþi Admin ise (Claim'lerde Admin yazýyorsa) doðrudan Admin/Index'e at
                if (User.IsInRole("Admin") || User.IsInRole("1"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                // Normal kullanýcý veya yöneticiyse Dashboard'a (veya Profilim'e) at
                return RedirectToAction("Dashboard", "Home");
            }

            // Giriþ yapmamýþ ziyaretçiler için o þeffaf cam efektli "Kayýt Ol" formunu (Index.cshtml) göster
            return View();
        }

        // 2. YENÝ EKLENEN DASHBOARD (Sadece Giriþ Yapanlar)
        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        // 3. ESKÝ KODUNDAN KORUNAN PROFIL SAYFASI (Sadece Giriþ Yapanlar)
        [Authorize]
        public IActionResult Profilim()
        {
            return View();
        }

        // 4. ESKÝ KODUNDAN KORUNAN YÖNETÝM PANELÝ (Sadece Adminler)
        // Not: Kullanýcýlarý listelediðimiz asýl yer "AdminController" olduðu için bu sayfayý 
        // farklý genel ayarlar (site logolarý, genel istatistikler vb.) için kullanabilirsin.
        [Authorize(Roles = "Admin")]
        public IActionResult YonetimPaneli()
        {
            return View();
        }
    }
}