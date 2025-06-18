namespace quiz.Domain.Dto;

public class ActiveQuiz
{
    public int AttemptId { get; set; } // The ID of the quiz attempt
    public DateTime StartedAt { get; set; } // The time when the quiz was started
    public int? DurationMinutes { get; set; } // The duration of the quiz in minutes
}