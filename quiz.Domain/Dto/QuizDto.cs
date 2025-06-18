namespace quiz.Domain.Dto;

public class QuizDto
{
    public int Id { get; set; }
    public string? Title { get; set; }  //
    public string? Description { get; set; }
    public int Totalmarks { get; set; }
    public int? Durationminutes { get; set; }
    public bool? Ispublic { get; set; }
    public DateTime? Startdate { get; set; }
    public DateTime? Enddate { get; set; }
    public int Categoryid { get; set; }
    public int Createdby { get; set; }

    public List<QuestionDto>? Questions { get; set; }
}
