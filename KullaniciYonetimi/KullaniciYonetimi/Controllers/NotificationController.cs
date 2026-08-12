using KullaniciYonetimi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

// Kendi projendeki namespace'leri buraya eklemeyi unutma (Models, Data vb.)

public class NotificationController : Controller
{
    private readonly AppDbContext _context;

    // Dependency Injection ile veritabanı bağlantımızı alıyoruz
    public NotificationController(AppDbContext context)
    {
        _context = context;
    }

    // 1. Admin'in Yeni Bildirim Gönderme Sayfası (GET)
    [HttpGet]
    [Authorize(Roles = "Admin")] // Güvenlik testimizi hatırlayarak yetkiyi ekliyoruz!
    public IActionResult Create()
    {
        return View();
    }

    // 2. Formdan Gelen Veriyi İşleme (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
     [Authorize(Roles = "Admin")]   // CSRF korumasını eklemeyi unutmuyoruz!
    public async Task<IActionResult> Create(string title, string description, NotificationType type, string targetAudience)
    {
        // 1. Adım: Ana bildirimi oluştur ve kaydet
        var newNotification = new Notification
        {
            Title = title,
            Description = description,
            Type = type,
            CreatedDate = DateTime.Now
        };

        _context.Notifications.Add(newNotification);
        await _context.SaveChangesAsync(); // Bildirim ID'sini alabilmek için önce kaydediyoruz

        // 2. Adım: Bu bildirimi kimlere göndereceğiz?
        var usersToNotify = new List<User>(); // Senin sistemindeki Kullanıcı modelinin adı neyse (AppUser, User vb.) onu yazmalısın.

        if (targetAudience == "All")
        {
            // Tüm kullanıcıları getir
            usersToNotify = await _context.Users.ToListAsync();
        }
        else
        {
            // İleride buraya "Sadece Standart Kullanıcılar" veya "Belirli Bir Rol" için filtreleme ekleyeceğiz.
        }

        // 3. Adım: Her bir kullanıcı için UserNotification köprü tablosuna kayıt at
        foreach (var user in usersToNotify)
        {
            var userNotification = new UserNotification
            {
                NotificationId = newNotification.Id, // Az önce kaydettiğimiz bildirimin ID'si
                UserId = user.Id,
                IsRead = false
            };
            _context.UserNotifications.Add(userNotification);
        }

        await _context.SaveChangesAsync();

        // İşlem bitince liste sayfasına yönlendir (Henüz listeyi yapmadık ama şimdilik Index yazalım)
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> MarkAsRead(int id)
    {
        // 1. Tıklayan kullanıcının ID'sini alıyoruz
        var userIdString = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account"); // Güvenlik önlemi
        }

        int userId = int.Parse(userIdString);

        // 2. Kullanıcının tıkladığı bu özel bildirimi veritabanında buluyoruz
        var userNotification = await _context.UserNotifications
            .FirstOrDefaultAsync(un => un.NotificationId == id && un.UserId == userId);

        // 3. Bildirim varsa ve henüz OKUNMADIYSA, onu okundu yapıyoruz
        if (userNotification != null && !userNotification.IsRead)
        {
            userNotification.IsRead = true;
            await _context.SaveChangesAsync(); // Değişikliği veritabanına kaydet
        }

        // 4. Kullanıcıyı bulunduğu sayfaya geri gönderiyoruz ki sayfa yenilensin ve rozet güncellensin
        string referer = Request.Headers["Referer"].ToString();
        return Redirect(string.IsNullOrEmpty(referer) ? "/" : referer);
    }


    public async Task<IActionResult> MyNotifications(int page = 1) // Parametre olarak sayfa numarasını alıyoruz
    {
        var userIdString = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = int.Parse(userIdString);
        int pageSize = 7; // Her sayfada gösterilecek bildirim sayısı

        // 1. Önce bu kullanıcıya ait TOPLAM bildirim sayısını buluyoruz
        var totalItems = await _context.UserNotifications
            .Where(un => un.UserId == userId)
            .CountAsync();

        // 2. Toplam sayfa sayısını hesaplıyoruz (Örn: 23 bildirim / 10 = 3 sayfa)
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // 3. İlgili sayfanın verilerini çekiyoruz (Skip ve Take sihirli kelimelerimiz)
        var notifications = await _context.UserNotifications
            .Include(un => un.Notification)
            .Where(un => un.UserId == userId)
            .OrderByDescending(un => un.Notification.CreatedDate)
            .Skip((page - 1) * pageSize) // Önceki sayfaların verilerini atla
            .Take(pageSize) // Sadece bu sayfanın verilerini al
            .ToListAsync();

        // 4. View'da butonları çizebilmek için sayfa bilgilerini gönderiyoruz
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(notifications);
    }

    public async Task<IActionResult> MarkAllAsRead()
    {
        // 1. Kullanıcı ID'sini alıyoruz
        var userIdString = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = int.Parse(userIdString);

        // 2. Bu kullanıcıya ait okunmamış (IsRead == false) TÜM bildirimleri buluyoruz
        var unreadNotifications = await _context.UserNotifications
            .Where(un => un.UserId == userId && un.IsRead == false)
            .ToListAsync();

        // 3. Eğer okunmamış bildirim varsa, hepsini dönüp true yapıyoruz
        if (unreadNotifications.Any())
        {
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            // 4. Veritabanını tek seferde güncelliyoruz (Performans için döngünün dışında kaydedilir)
            await _context.SaveChangesAsync();
        }

        // İşlem bitince kullanıcıyı tekrar bildirimlerim sayfasına yönlendiriyoruz
        return RedirectToAction("MyNotifications");
    }
}

