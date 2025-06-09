namespace quiz.Domain.ViewModels;

public class CreateQuizViewModel
{
      public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int Totalmarks { get; set; }

    public int? Durationminutes { get; set; }

    public bool? Ispublic { get; set; }

    public DateTime? Startdate { get; set; }

    public DateTime? Enddate { get; set; }

    public int Categoryid { get; set; }

}
