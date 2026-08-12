using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KullaniciYonetimi.Models;
using System.Security.Claims;

namespace KullaniciYonetimi.ViewComponents
{

    public class DynamicMenuViewComponent : Microsoft.AspNetCore.Mvc.ViewComponent
    {
        private readonly AppDbContext _context;

        public DynamicMenuViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // 1. Sisteme giriş yapmış kullanıcının rol adını alıyoruz
            var roleName = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(roleName))
            {
                return View(new List<Menu>()); // Rol bulunamazsa boş liste döner
            }

            // 2. Rol adına göre veritabanından Rolün Id'sini buluyoruz
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null)
            {
                return View(new List<Menu>());
            }

            // 3. Bu rolün yetkili olduğu menülerin Id listesini çekiyoruz
            var authorizedMenuIds = await _context.RoleMenus
                .Where(rm => rm.RoleId == role.Id)
                .Select(rm => rm.MenuId)
                .ToListAsync();

            // 4. Yetkisi olan aktif menüleri (alt menüleriyle beraber) hiyerarşik olarak çekiyoruz
            var menus = await _context.Menus
                .Where(m => m.IsActive && authorizedMenuIds.Contains(m.Id))
                // Alt menüleri de çekerken yetki kontrolü yapıp sıraya diziyoruz
                .Include(m => m.SubMenus.Where(sm => sm.IsActive && authorizedMenuIds.Contains(sm.Id)).OrderBy(sm => sm.Order))
                .OrderBy(m => m.Order)
                .ToListAsync();

            // View'a sadece "Ana Menü" olanları gönderiyoruz (Alt menüler zaten içlerinde yer alıyor)
            var mainMenus = menus.Where(m => m.ParentId == null).ToList();

            return View(mainMenus);
        }
    }
}