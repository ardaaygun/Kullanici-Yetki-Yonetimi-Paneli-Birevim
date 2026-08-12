namespace KullaniciYonetimi.Models
{
    public class UserNotification
    {
       

        public int Id { get; set; }

        // Hangi bildirim?
        public int NotificationId { get; set; }
        public Notification Notification { get; set; }

        // Hangi kullanıcıya gitti? 
        public int UserId { get; set; }
        public User User { get; set; }

        // Okundu / Okunmadı Durumu
        public bool IsRead { get; set; } = false;
        public DateTime? ReadDate { get; set; } // Okunduğu anın tarihini de tutmak her zaman iyidir


    }
}