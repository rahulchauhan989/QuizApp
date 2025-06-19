using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.Dto
{
    public class UpdateQuizDto
    {
        [Required(ErrorMessage = "Quiz ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quiz ID must be a positive integer.")]
        public int Id { get; set; }  

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string? Title { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Total marks are required.")]
        [Range(1, 180, ErrorMessage = "Total marks must be between 1 and 180.")]
        public int? Totalmarks { get; set; }

        [Required(ErrorMessage = "Duration in minutes is required.")]
        public int? Durationminutes { get; set; }
        public bool? Ispublic { get; set; }
        public int? Categoryid { get; set; }
    }
}
