namespace quiz.Domain.ViewModels;

public class UserAnswerDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool? IsCorrect { get; set; }
}
