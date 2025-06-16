namespace quiz.Domain.ViewModels
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Role { get; set; }
    }
}
