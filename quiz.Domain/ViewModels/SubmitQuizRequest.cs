using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.ViewModels;

public class SubmitQuizRequest
{
    [Required(ErrorMessage = "UserId is required.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "QuizId is required.")]
    public int QuizId { get; set; }

    [Required(ErrorMessage = "CategoryId is required.")]
    public int categoryId { get; set; }
    public List<SubmittedAnswer> Answers { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
}

public class SubmittedAnswer
{
    [Required(ErrorMessage = "QuestionId is required.")]
    public int QuestionId { get; set; }
    [Required(ErrorMessage = "OptionId is required.")]
    public int OptionId { get; set; }
}

