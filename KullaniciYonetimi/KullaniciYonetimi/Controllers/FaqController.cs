using KullaniciYonetimi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Identity.Client;

namespace KullaniciYonetimi.Controllers
{
    public class FaqController : Controller
    {

        private readonly AppDbContext _context;

        public FaqController(AppDbContext context)
        {
            _context = context;

        }

        public IActionResult Index()
        {

            var ActiveCategories = _context.FaqCategories.Where(c => c.IsActive == true).ToList();

            return View(ActiveCategories);

        }


        [HttpGet]
        public IActionResult CreateCategory()
        {

            return View();

        }

        [HttpPost]

        public IActionResult CreateCategory(FaqCategory category)
        {
            _context.FaqCategories.Add(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditCategory(int id)
        {

            var category = _context.FaqCategories.Find(id);
            if (category != null)
            {

                return View(category);
            }
            else
            {
                return NotFound();
            }


        }

        [HttpPost]
        public IActionResult EditCategory(FaqCategory category)
        {

            _context.FaqCategories.Update(category);

            _context.SaveChanges();
            return RedirectToAction("Index");


        }

        [HttpPost]

        public IActionResult DeleteCategory(int id)
        {
            var category = _context.FaqCategories.Find(id);
            if (category != null)
            {
                _context.FaqCategories.Remove(category);
                _context.SaveChanges();

                return RedirectToAction("Index");

            }
            else
            {
                return NotFound();



            }
        }


        [HttpGet]

        public IActionResult Questions (int categoryId) {
            var sorular = _context.Faqs.Where(f => f.CategoryId == categoryId).ToList();
            var kategori = _context.FaqCategories.Find(categoryId);
            ViewBag.CategoryName = kategori.CategoryName;
            ViewBag.CategoryId = kategori.Id;
            return View(sorular);
        }


        [HttpGet]

    
        public IActionResult CreateQuestion(int categoryId)
        {
            ViewBag.CategoryId = categoryId;
            return View();
        }

        [HttpPost]
        public IActionResult CreateQuestion(Faq soru)
        {
            _context.Faqs.Add(soru);
            _context.SaveChanges();
            return RedirectToAction("Questions", new { categoryId = soru.CategoryId });
        }

        [HttpGet]
        public IActionResult EditQuestion(int id)
        {
            var soru = _context.Faqs.Find(id);
            if (soru != null)
            {
                // Geri dön butonunda kullanmak için kategori ID'sini View'a taşıyoruz
                ViewBag.CategoryId = soru.CategoryId;
                return View(soru);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult EditQuestion(Faq soru)
        {
            _context.Faqs.Update(soru);
            _context.SaveChanges();

            // İşlem bitince sorunun ait olduğu kategori listesine geri dönüyoruz
            return RedirectToAction("Questions", new { categoryId = soru.CategoryId });
        }

        [HttpPost]
        public IActionResult DeleteQuestion(int id)
        {
            var soru = _context.Faqs.Find(id);
            if (soru != null)
            {
                // Yönlendirme için Kategori ID'sini silmeden önce bir kenara not alıyoruz
                int catId = soru.CategoryId;

                _context.Faqs.Remove(soru);
                _context.SaveChanges();

                return RedirectToAction("Questions", new { categoryId = catId });
            }
            return NotFound();
        }
    }

}

