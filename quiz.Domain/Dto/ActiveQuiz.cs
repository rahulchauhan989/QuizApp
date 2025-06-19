namespace quiz.Domain.Dto;

public class ActiveQuiz
{
    public int AttemptId { get; set; } 
    public DateTime StartedAt { get; set; }
    public int? DurationMinutes { get; set; } 
}