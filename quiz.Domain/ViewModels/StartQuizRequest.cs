namespace quiz.Domain.ViewModels;

public class StartQuizRequest
{
    
    public int QuizId { get; set; } // The ID of the quiz the user wants to start
    public int UserId { get; set; } // The ID of the user starting the quiz

    public int categoryId { get; set; } // The ID of the category the quiz belongs to
}
