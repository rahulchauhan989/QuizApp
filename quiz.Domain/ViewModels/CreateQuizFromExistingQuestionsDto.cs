namespace quiz.Domain.ViewModels;

public class CreateQuizFromExistingQuestionsDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Totalmarks { get; set; }
    public int? Durationminutes { get; set; }
    public bool? Ispublic { get; set; }
    // public DateTime? Startdate { get; set; }
    // public DateTime? Enddate { get; set; }
    public int Categoryid { get; set; }
    public int Createdby { get; set; }
    public List<int> QuestionIds { get; set; } = new List<int>(); // List of existing question IDs
}