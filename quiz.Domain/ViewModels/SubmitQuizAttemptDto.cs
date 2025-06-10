namespace quiz.Domain.ViewModels;

public class SubmitQuizAttemptDto
{
    public int UserId { get; set; }
    public int QuizId { get; set; }
    public List<AnswerDto>? Answers { get; set; }
}

public class AnswerDto
{
    public int QuestionId { get; set; }
    public int OptionId { get; set; }
}