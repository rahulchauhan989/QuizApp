namespace quiz.Domain.ViewModels;

public class QuestionDto
{
    public int Id { get; set; }
    // public int QuizId { get; set; }

    public int Categoryid { get; set; }
    public string? Text { get; set; }  //
    public int? Marks { get; set; }
    public string? Difficulty { get; set; }

    public int createdBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public List<OptionDto>? Options { get; set; }  //
}

public class OptionDto
{
    public int Id { get; set; }
    public string? Text { get; set; }  //
    public bool IsCorrect { get; set; }
}


