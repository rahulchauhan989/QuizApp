namespace quiz.Domain.Dto;
public class QuestionEditDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Marks { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int Categoryid { get; set; }
    public List<OptionEditDto> Options { get; set; } = new();
}


public class OptionEditDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}