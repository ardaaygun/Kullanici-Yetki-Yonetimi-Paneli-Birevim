namespace KullaniciYonetimi.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate {  get; set; }
        public NotificationType Type { get; set; }


        public ICollection<UserNotification> UserNotifications { get; set; } //navigation 



    }
}
