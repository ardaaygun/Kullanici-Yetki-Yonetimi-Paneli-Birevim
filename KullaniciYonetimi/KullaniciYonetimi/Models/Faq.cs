namespace KullaniciYonetimi.Models
{
    public class Faq

    {

        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        
        public int Rank { get; set; }

        public int ViewNumber { get; set; }

        public bool IsActive { get; set; }

        public int CategoryId { get; set; }


        public FaqCategory Category { get; set; }
    }
}
