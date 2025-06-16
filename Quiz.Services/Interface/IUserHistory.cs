using quiz.Domain.ViewModels;

namespace Quiz.Services.Interface;

public interface IUserHistory
{
    Task<List<UserQuizHistoryDto>> GetUserQuizHistoryAsync(int userId);
}
