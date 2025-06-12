using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.ViewModels;
public class LoginModel
{
    [EmailAddress]

    [Required(ErrorMessage = "Email is required ")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? password { get; set; }

    public bool RememberMe { get; set; }
}
