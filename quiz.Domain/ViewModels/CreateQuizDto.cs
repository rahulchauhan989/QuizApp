namespace quiz.Domain.ViewModels;

public class CreateQuizDto
{
    public string? Title { get; set; }   //
    public string? Description { get; set; }
    public int Totalmarks { get; set; }
    public int? Durationminutes { get; set; }
    public bool? Ispublic { get; set; }
    public DateTime? Startdate { get; set; }
    public DateTime? Enddate { get; set; }
    public int Categoryid { get; set; }
    public int Createdby { get; set; }
    public List<int>? TagIds { get; set; } // If we want to add tags by ID
    public List<CreateQuestionDto>? Questions { get; set; } 
}

public class CreateQuestionDto
{
    public string? Text { get; set; }  //
    public int Marks { get; set; }
    public string? Difficulty { get; set; }
    public List<CreateOptionDto>? Options { get; set; }
}

public class CreateOptionDto
{
    public string? Text { get; set; }  //
    public bool IsCorrect { get; set; }
}