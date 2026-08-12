namespace KullaniciYonetimi.ViewComponent;
using KullaniciYonetimi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Kullanıcı ID'sini almak için
// Kendi projendeki namespace'leri (Models, Data vs.) eklemeyi unutma

public class NotificationBellViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public NotificationBellViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // 1. Sisteme giriş yapmış kullanıcının ID'sini alıyoruz
        // Not: Eğer senin giriş (Login) sisteminde ID farklı bir Claim ile tutuluyorsa burayı ona göre güncellemelisin.
        var userIdString = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return View(new List<UserNotification>()); // Kullanıcı giriş yapmamışsa boş liste döndür
        }

        int userId = int.Parse(userIdString);

        // 2. Bu kullanıcıya ait, henüz okunmamış en güncel 5 bildirimi veritabanından çekiyoruz
        var unreadNotifications = await _context.UserNotifications
            .Include(un => un.Notification) // Navigation Property sihrini kullanıyoruz!
            .Where(un => un.UserId == userId && un.IsRead == false)
            .OrderByDescending(un => un.Notification.CreatedDate)
            .Take(5) // Açılır menü çok uzamasın diye son 5'i alıyoruz
            .ToListAsync();

        // 3. Rozet (Badge) için okunmamış toplam bildirim sayısını ViewBag ile View'a gönderiyoruz
        int unreadCount = await _context.UserNotifications
            .CountAsync(un => un.UserId == userId && un.IsRead == false);

        ViewBag.UnreadCount = unreadCount;

        // 4. Veriyi View'a gönder
        return View(unreadNotifications);
    }
}
