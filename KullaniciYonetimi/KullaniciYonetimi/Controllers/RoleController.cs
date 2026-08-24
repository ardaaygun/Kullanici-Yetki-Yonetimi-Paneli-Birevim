using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KullaniciYonetimi.Models;

namespace KullaniciYonetimi.Controllers
{
    [Authorize(Roles = "Admin,3")] 
    public class RoleController : Controller
    {
        private readonly AppDbContext _context;

        public RoleController(AppDbContext context)
        {
            _context = context;
        }

        // Rollerin Listelendiği Sayfa
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles.ToListAsync();
            return View(roles);
        }

        // 2. Yeni Rol Ekleme Sayfası (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. Yeni Rol Ekleme İşlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            // İlgili navigation property'leri (varsa) yoksayalım
            ModelState.Remove("RoleMenus");
            ModelState.Remove("Users"); 

            if (ModelState.IsValid)
            {
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // 4. Rol Güncelleme Sayfası (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            return View(role);
        }

        // 5. Rol Güncelleme İşlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Role role)
        {
            if (id != role.Id) return NotFound();

            ModelState.Remove("RoleMenus");
            ModelState.Remove("Users");

            if (ModelState.IsValid)
            {
                _context.Update(role);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // 6. Rol Silme Onay Sayfası (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            return View(role);
        }

        // 7. Rol Silme İşlemi (POST - Veritabanından tamamen siler)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}