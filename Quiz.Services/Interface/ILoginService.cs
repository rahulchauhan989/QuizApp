using quiz.Domain.ViewModels;

namespace Quiz.Services.Interface;

public interface ILoginService
{
    Task<bool> ValidateUserAsync(string email, string password);

    Task<string> GenerateToken(LoginModel request);

    Task<String> RegisterUserAsync(RegistrationViewModel request);
}
