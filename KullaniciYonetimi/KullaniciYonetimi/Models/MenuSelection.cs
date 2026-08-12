namespace KullaniciYonetimi.Models
{
    public class MenuSelection
    {
        public int MenuId { get; set; }
        public string Title { get; set; }
        public string ParentName { get; set; } // Hangi üst menüye bağlı olduğunu göstermek için
        public bool IsSelected { get; set; }
    }
}