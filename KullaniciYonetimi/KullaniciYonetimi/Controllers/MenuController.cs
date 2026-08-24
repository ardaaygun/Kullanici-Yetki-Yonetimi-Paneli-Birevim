using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KullaniciYonetimi.Models;

namespace KullaniciYonetimi.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class MenuController : Controller
    {
        private readonly AppDbContext _context;

        public MenuController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Tüm Menülerin Listelendiği Sayfa
        public async Task<IActionResult> Index()
        {
            // Include ile bağlı olduğu üst menüyü de (ParentMenu) getiriyoruz
            var menus = await _context.Menus
                .Include(m => m.ParentMenu)
                .OrderBy(m => m.ParentId) // Önce ana menüler, sonra alt menüler sıralansın
                .ThenBy(m => m.Order)     // Kendi içlerinde de "Sıra" (Order) numarasına göre dizilsin
                .ToListAsync();

            return View(menus);
        }

        // Yeni Menü Ekleme 
        public async Task<IActionResult> Create()
        {
            // Eğer eklenecek menü bir "Alt Menü" olacaksa, adminin seçebilmesi için mevcut ana menüleri View'a gönderiyoruz
            var parentMenus = await _context.Menus.Where(m => m.IsActive).ToListAsync();
            ViewBag.ParentMenus = new SelectList(parentMenus, "Id", "Title");

            return View();
        }

        // Yeni Menü Ekleme İşlemi 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Menu menu)
        {
            // Model doğrulamasına takılmaması için ilişkisel (navigation) özellikleri yoksayıyoruz
            ModelState.Remove("ParentMenu");
            ModelState.Remove("SubMenus");
            ModelState.Remove("RoleMenus");

            if (ModelState.IsValid)
            {
                _context.Menus.Add(menu);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Menü başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            // Hata olursa dropdown listesini tekrar doldurup formu geri döndür
            var parentMenus = await _context.Menus.Where(m => m.IsActive).ToListAsync();
            ViewBag.ParentMenus = new SelectList(parentMenus, "Id", "Title");

            return View(menu);
        }

        // 4. Menü Güncelleme Sayfası 
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            // Dropdown için diğer aktif menüleri getiriyoruz (Kendisini üst menü olarak seçemesin diye kendi ID'sini hariç tutuyoruz)
            var parentMenus = await _context.Menus.Where(m => m.IsActive && m.Id != id).ToListAsync();
            ViewBag.ParentMenus = new SelectList(parentMenus, "Id", "Title", menu.ParentId);

            return View(menu);
        }

        // Menü Güncelleme İşlemi 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Menu menu)
        {
            if (id != menu.Id)
            {
                return NotFound();
            }

            // Model doğrulamasına takılmaması için ilişkisel özellikleri yoksayıyoruz
            ModelState.Remove("ParentMenu");
            ModelState.Remove("SubMenus");
            ModelState.Remove("RoleMenus");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(menu);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Menü başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Menus.Any(e => e.Id == menu.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var parentMenus = await _context.Menus.Where(m => m.IsActive && m.Id != menu.Id).ToListAsync();
            ViewBag.ParentMenus = new SelectList(parentMenus, "Id", "Title", menu.ParentId);
            return View(menu);
        }

        // 6. Rollere Menü Atama Sayfası 
        public async Task<IActionResult> RoleAssignment(int? roleId)
        {
            // Sistemdeki tüm rolleri dropdown için çekiyoruz
            var roles = await _context.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "RoleName", roleId);

            var model = new List<MenuSelection>();

            // Eğer dropdown'dan bir rol seçilmişse, menüleri getireceğiz
            if (roleId.HasValue)
            {
                var allMenus = await _context.Menus
                    .Include(m => m.ParentMenu)
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.ParentId)
                    .ThenBy(m => m.Order)
                    .ToListAsync();

                // Seçilen role atanmış mevcut menülerin Id'lerini alıyoruz
                var roleMenus = await _context.RoleMenus
                    .Where(rm => rm.RoleId == roleId.Value)
                    .Select(rm => rm.MenuId)
                    .ToListAsync();

                foreach (var menu in allMenus)
                {
                    model.Add(new MenuSelection
                    {
                        MenuId = menu.Id,
                        Title = menu.Title,
                        ParentName = menu.ParentMenu?.Title,
                        IsSelected = roleMenus.Contains(menu.Id) 
                    });
                }
            }

            ViewBag.SelectedRoleId = roleId;
            return View(model);
        }

        // Rollere Menü Atama İşlemi 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoleMenus(int roleId, List<int> selectedMenus)
        {
            if (roleId == 0) return RedirectToAction(nameof(RoleAssignment));

            // 1. Bu role ait eski yetkileri tamamen temizliyoruz (Temiz bir sayfa açıyoruz)
            var existingMenus = _context.RoleMenus.Where(rm => rm.RoleId == roleId);
            _context.RoleMenus.RemoveRange(existingMenus);

            // 2. Seçili olan yeni checkbox'ları veritabanına ekliyoruz
            if (selectedMenus != null && selectedMenus.Any())
            {
                var newAssignments = selectedMenus.Select(menuId => new RoleMenu
                {
                    RoleId = roleId,
                    MenuId = menuId
                });
                _context.RoleMenus.AddRange(newAssignments);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Rol yetkileri başarıyla güncellendi.";

            // İşlem bitince aynı rolü seçili tutarak sayfayı yeniliyoruz
            return RedirectToAction(nameof(RoleAssignment), new { roleId = roleId });
        }

        // 8. Menü Silme Onay Sayfası (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound();

            return View(menu);
        }

        // 9. Menü Silme İşlemi (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu != null)
            {
                // Menüyü veritabanından siliyoruz (Alt menüleri veya yetkileri varsa EF Core yapılandırmana göre otomatik silinir veya hata fırlatır)
                _context.Menus.Remove(menu);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}