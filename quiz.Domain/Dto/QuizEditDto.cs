namespace quiz.Domain.Dto;

public class QuizEditDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Totalmarks { get; set; }
    public int Durationminutes { get; set; }
    public bool Ispublic { get; set; }
    public int Categoryid { get; set; }
    public List<QuestionEditDto> Questions { get; set; } = new();
}
