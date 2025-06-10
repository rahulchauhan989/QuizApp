using quiz.Domain.DataModels;

namespace quiz.Repo.Interface;

public interface IUserQuizAttemptRepository
{
    Task<Userquizattempt?> GetAttemptByUserAndQuizAsync(int userId, int quizId, int categoryId);
    Task<int> CreateAttemptAsync(Userquizattempt attempt);

    Task UpdateAttemptAsync(Userquizattempt attempt);
}
