namespace quiz.Domain.Dto;

public class StartQuizRequest
{
    
    public int QuizId { get; set; } 
    public int UserId { get; set; } 

    public int categoryId { get; set; }
}
