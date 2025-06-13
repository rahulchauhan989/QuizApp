using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.ViewModels;

public class CreateQuizDto
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
    // public DateTime? Startdate { get; set; }
    // public DateTime? Enddate { get; set; }
    [Required(ErrorMessage = "Category ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0.")]
    public int Categoryid { get; set; }
    // public int Createdby { get; set; }
    public List<int>? TagIds { get; set; } 
    public List<CreateQuestionDto>? Questions { get; set; } 
}

public class CreateQuestionDto
{
    [Required(ErrorMessage = "Quetion Text is required.")]
    [MaxLength(500, ErrorMessage = "Question text cannot exceed 500 characters.")]
    public string? Text { get; set; } 

    [Required(ErrorMessage = "Marks are required.")] 
    [Range(1, 4, ErrorMessage = "Marks must be between 1 and 4")]
    public int Marks { get; set; }
    public string? Difficulty { get; set; }
    
    
    public List<CreateOptionDto>? Options { get; set; }
}

public class CreateOptionDto
{
    [Required(ErrorMessage = "Option Text is required.")]
    public string? Text { get; set; }  
    public bool IsCorrect { get; set; }
}