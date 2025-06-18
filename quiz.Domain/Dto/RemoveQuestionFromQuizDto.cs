using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.Dto;

public class RemoveQuestionFromQuizDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "QuizId must be a positive integer.")]
    public int QuizId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "QuestionId must be a positive integer.")]
    public int QuestionId { get; set; }
}

