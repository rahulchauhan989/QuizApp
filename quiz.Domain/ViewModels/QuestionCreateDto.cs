namespace quiz.Domain.ViewModels;

public class QuestionCreateDto
{
    // public int QuizId { get; set; }

    public int Categoryid { get; set; }
    public string? Text { get; set; }  //
    public int Marks { get; set; } = 1;
    public string? Difficulty { get; set; }  //

    public List<OptionCreateDto>? Options { get; set; }  //
}

public class OptionCreateDto
{
    public string? Text { get; set; }  //
    public bool IsCorrect { get; set; }
}

