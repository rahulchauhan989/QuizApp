namespace quiz.Domain.Dto;

public class CreateQuizzDto
{
     public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int Totalmarks { get; set; }

    public int? Durationminutes { get; set; }

    public bool? Ispublic { get; set; }

    public bool? Isdeleted { get; set; }


    public int Categoryid { get; set; }

}
