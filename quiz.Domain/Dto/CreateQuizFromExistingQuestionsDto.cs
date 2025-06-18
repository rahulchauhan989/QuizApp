using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.Dto;

public class CreateQuizFromExistingQuestionsDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Total marks are required.")]
    [Range(1, 100, ErrorMessage = "Total marks must be between 1 and 100.")]
    public int Totalmarks { get; set; }

    [Required(ErrorMessage = "Duration in minutes is required.")]
    [Range(1,180, ErrorMessage = "Duration must be between 1 and 180 minutes.")]
    public int? Durationminutes { get; set; }
    public bool? Ispublic { get; set; }
    // public DateTime? Startdate { get; set; }
    // public DateTime? Enddate { get; set; }
    public int Categoryid { get; set; }
    public int Createdby { get; set; }
    public List<int> QuestionIds { get; set; } = new List<int>(); // List of existing question IDs
}