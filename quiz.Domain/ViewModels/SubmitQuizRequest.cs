namespace quiz.Domain.ViewModels;

public class SubmitQuizRequest
{
    public int UserId { get; set; }
    public int QuizId { get; set; }

    public int categoryId { get; set; }
    public List<SubmittedAnswer> Answers { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
}

public class SubmittedAnswer
{
    public int QuestionId { get; set; }
    public int OptionId { get; set; }
}

