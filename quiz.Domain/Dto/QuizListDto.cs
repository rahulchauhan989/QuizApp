namespace quiz.Domain.Dto;

public class QuizListDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int TotalMarks { get; set; }
    public int? DurationMinutes { get; set; }
    public bool? IsPublic { get; set; }
    public string? CategoryName { get; set; }
}