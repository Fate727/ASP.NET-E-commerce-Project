namespace Techhive.Models
{
    public class FeedbackListViewModel
    {
        public IEnumerable<ContactMessage> Feedbacks { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
