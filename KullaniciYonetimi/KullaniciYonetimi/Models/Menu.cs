using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KullaniciYonetimi.Models
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Menü başlığı zorunludur.")]
        [MaxLength(100)]
        public string Title { get; set; } // Menü Adı (örn: Kullanıcı Yönetimi)

        [MaxLength(255)]
        public string Url { get; set; } // Tıklanınca gideceği adres (örn: /Admin/Index)

        [MaxLength(100)]
        public string Icon { get; set; } // FontAwesome ikon class'ı (örn: fa-solid fa-users)

        public int? ParentId { get; set; } // Üst menü ise null, alt menü ise bağlı olduğu ana menünün Id'si

        public int Order { get; set; } // Menülerin ekrandaki sıralaması

        public bool IsActive { get; set; } = true; // Aktif/Pasif durumu

        // Entity Framework Navigation Properties (Tablo İlişkileri)
        [ForeignKey("ParentId")]
        public virtual Menu ParentMenu { get; set; }
        public virtual ICollection<Menu> SubMenus { get; set; }
        public virtual ICollection<RoleMenu> RoleMenus { get; set; }
    }
}