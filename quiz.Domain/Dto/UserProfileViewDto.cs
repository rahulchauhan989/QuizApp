namespace quiz.Domain.Dto
{
    public class UserProfileViewDto
    {
        public int Id { get; set; }
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Role { get; set; }
    }
}
