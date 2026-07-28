using Microsoft.AspNetCore.Mvc;
using KullaniciYonetimi.Models.ViewModels;

namespace KullaniciYonetimi.Controllers
{
    public class AccountController : Controller
    {
        // /Account/Login adresine girildiğinde Login.cshtml'i açar
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // /Account/Register adresine girildiğinde Register.cshtml'i açar
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Formdaki "Hesabımı Oluştur" butonuna basıldığında tetiklenen metod
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Form kurallarına uyulmadıysa aynı sayfayı formu doldurarak geri gösterir
                return View(model);
            }

            // TODO: İleride veritabanı kayıt mantığını buraya bağlayacağız.

            // Başarılı kayıt bildirimi ve Giriş sayfasına yönlendirme
            TempData["SuccessMessage"] = "Kayıt işleminiz başarıyla tamamlandı!";
            return RedirectToAction("Login", "Account");
        }
    }
}