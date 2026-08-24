namespace KullaniciYonetimi.Models
{
    public class FaqCategory
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Faq> Faqs { get; set; } //FAQ kategorisinde birden fazla soru olacağında colletın ile tutuldu.
    }
}
