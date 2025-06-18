using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.Dto;

public class QuestionCreateDto
{
    [Required(ErrorMessage = "Categoryid is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Categoryid must be greater than 0.")]
    public int Categoryid { get; set; }

    [Required(ErrorMessage = "Question Text is required.")]
    [MaxLength(200, ErrorMessage = "Question text cannot exceed 200 characters.")]
    public string? Text { get; set; }

    [Required(ErrorMessage = "Marks are required.")]
    [Range(1, 4, ErrorMessage = "Marks must be between 1 and 4")]
    public int Marks { get; set; } = 1;

    [MaxLength(10, ErrorMessage = "Difficulty cannot exceed 10 characters.")]
    public string? Difficulty { get; set; }
    public List<OptionCreateDto>? Options { get; set; }
}

public class OptionCreateDto
{
    [Required(ErrorMessage = "Option Text is required.")]
    [MaxLength(100, ErrorMessage = "Option text cannot exceed 100 characters.")]
    public string? Text { get; set; }
    public bool IsCorrect { get; set; }
}

