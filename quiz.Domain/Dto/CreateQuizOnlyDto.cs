using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.Dto;

public class CreateQuizOnlyDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string? Title { get; set; }
    [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string? Description { get; set; }
    [Required(ErrorMessage = "Total marks are required.")]
    [Range(1, 100, ErrorMessage = "Total marks must be between 1 and 100.")]
    public int Totalmarks { get; set; }

    [Required(ErrorMessage = "Duration in minutes is required.")]
    [Range(1, 180, ErrorMessage = "Duration must be between 1 and 180 minutes.")]
    public int? Durationminutes { get; set; }
    public bool? Ispublic { get; set; }
    
    [Required(ErrorMessage = "Category ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0.")]
    public int Categoryid { get; set; }
    public List<int>? TagIds { get; set; }
    
}


public class AddQuestionToQuizDto
{
    [Required(ErrorMessage = "Quiz ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Quiz ID must be greater than 0.")]
    public int QuizId { get; set; }
    public List<int>? ExistingQuestionIds { get; set; } 

    [MaxLength(200, ErrorMessage = "Question text cannot exceed 200 characters.")]
    public string? Text { get; set; }

    [Range(1, 4, ErrorMessage = "Marks must be between 1 and 4.")]
    public int? Marks { get; set; }

    [MaxLength(10, ErrorMessage = "Difficulty cannot exceed 10 characters.")]
    [RegularExpression(@"^(Easy|Medium|Hard|easy|medium|hard)$", ErrorMessage = "Difficulty must be either 'Easy', 'Medium', or 'Hard'.")]
    public string? Difficulty { get; set; }
    public List<AddOptionDto>? Options { get; set; }
}


public class AddOptionDto
{
    [MaxLength(200, ErrorMessage = "Option text cannot exceed 200 characters.")]
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}