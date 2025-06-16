using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;

namespace quiz.Repo.Interface;

public interface ILoginRepo
{
    Task<bool> ValidateUserAsync(string email, string password);

    Task<User> GetUserByEmailAsync(string email);

    Task<string> RegisterUserAsync(User user);

    Task<User?> GetUserByIdAsync(int userId);
}
