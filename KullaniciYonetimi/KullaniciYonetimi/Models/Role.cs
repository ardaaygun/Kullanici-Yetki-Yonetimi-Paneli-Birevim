using System.Collections.Generic;

namespace KullaniciYonetimi.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; } //Admin,Manager,NormalUser
       
        // Bir rolün birden fazla kullanıcısı olabileceğini sisteme söylüyoruz
        public ICollection<User> Users { get; set; }
    }
}