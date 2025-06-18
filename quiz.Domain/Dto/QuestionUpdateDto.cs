using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.Dto;

public class QuestionUpdateDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Category ID is required.")]
    [Range(1, int.MaxValue)]
    public int Categoryid { get; set; }

    [Required(ErrorMessage = "Question text is required.")]
    [MaxLength(200)]
    public string? Text { get; set; }

    [Range(1, 4)]
    public int Marks { get; set; }

    [MaxLength(10)]
    public string? Difficulty { get; set; }

    public List<OptionCreateDto>? Options { get; set; }
}

