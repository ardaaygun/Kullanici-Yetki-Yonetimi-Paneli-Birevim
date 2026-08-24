namespace KullaniciYonetimi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public int RoleId { get; set; }

        // (Navigation Property)
        public Role Role { get; set; }
    }


}
