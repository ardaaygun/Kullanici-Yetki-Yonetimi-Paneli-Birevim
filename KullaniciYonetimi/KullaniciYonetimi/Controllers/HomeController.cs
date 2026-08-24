using KullaniciYonetimi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace KullaniciYonetimi.Controllers
{
    public class HomeController : Controller
    {

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

                
                return RedirectToAction("Dashboard", "Home");
            }

            
            return View();
        }

        // DASHBOARD 
        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        // 3. PROFIL SAYFASI 
        [Authorize]
        public IActionResult Profilim()
        {
            return View();
        }
         
        [Authorize(Roles = "Admin")]
        public IActionResult YonetimPaneli()
        {
            return View();
        }
    }
}