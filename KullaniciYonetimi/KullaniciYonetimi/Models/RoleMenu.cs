using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KullaniciYonetimi.Models
{
    public class RoleMenu
    {
        [Key]
        public int Id { get; set; }

        [Required]
       
        public int RoleId { get; set; }

        public int MenuId { get; set; }

        // Navigation Property
        [ForeignKey("MenuId")]
        public virtual Menu Menu { get; set; }
    }
}