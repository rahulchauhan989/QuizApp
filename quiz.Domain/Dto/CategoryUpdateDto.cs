using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.Dto;

public class CategoryUpdateDto
{
    [Required]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string? Name { get; set; }
    [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string? Description { get; set; }
}
