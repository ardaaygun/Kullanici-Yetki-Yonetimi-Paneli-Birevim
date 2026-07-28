namespace KullaniciYonetimi.Models.ViewModels
{
    // Admin listesinde kullanıcıları göstermek için kullandığımız model
    public class UserViewModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? RoleName { get; set; }
    }

    // Kayıt ol formundan gelen verileri karşılayacak yeni modelimiz
    public class RegisterViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public bool IsTermsAccepted { get; set; }
    }
}