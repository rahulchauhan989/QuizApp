namespace quiz.Domain.ViewModels;

public class UserQuizHistoryDto
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int? Score { get; set; }
    public int? TimeSpent { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool? IsSubmitted { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<UserAnswerDto> UserAnswers { get; set; } = new List<UserAnswerDto>();
}