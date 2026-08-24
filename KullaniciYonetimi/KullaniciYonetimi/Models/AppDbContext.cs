using Microsoft.EntityFrameworkCore;

namespace KullaniciYonetimi.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
       
        public DbSet<Role> Roles { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<UserNotification> UserNotifications { get; set; }

        public DbSet<Menu> Menus { get; set; }
       
        public DbSet<RoleMenu> RoleMenus { get; set; }

        public DbSet<Faq> Faqs { get; set; }

        public DbSet<FaqCategory> FaqCategories { get; set; }


    }
}