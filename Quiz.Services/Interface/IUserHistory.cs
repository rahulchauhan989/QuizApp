using quiz.Domain.ViewModels;

namespace Quiz.Services.Interface;

public interface IUserHistory
{
    Task<List<UserQuizHistoryDto>> GetUserQuizHistoryAsync(int userId);
    Task<bool> isUserExistAsync(int userId);
    Task<ValidationResult> ValidateUserIdAsync(int userId);
}
