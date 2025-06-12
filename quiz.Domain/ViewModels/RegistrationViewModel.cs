using System.ComponentModel.DataAnnotations;

namespace quiz.Domain.ViewModels;

public class RegistrationViewModel
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
    public string? Name { get; set; }

    [EmailAddress]

    [Required(ErrorMessage = "Email is required ")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Password must be like eg.Abc@123 and length should be 8 to 15")]

    public string? password { get; set; }

    [Required(ErrorMessage = "Confirm Password is required")]
    [Compare("password", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword { get; set; }
}
