using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.ViewModels;
public class LoginModel
{
    [EmailAddress]

    [Required(ErrorMessage = "Email is required ")]
    [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? password { get; set; }


    public bool RememberMe { get; set; }
}
