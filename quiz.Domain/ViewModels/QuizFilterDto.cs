namespace quiz.Domain.ViewModels;

public class QuizFilterDto
{
    public int? CategoryId { get; set; }
    public string? TitleKeyword { get; set; }
    public bool? IsPublic { get; set; }
}

