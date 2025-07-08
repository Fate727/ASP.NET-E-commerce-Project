namespace Techhive.Models
{
    public class UserVcountViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int TotalViews { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsCurrentUser { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; }
    }
}
