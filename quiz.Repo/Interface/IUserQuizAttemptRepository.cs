using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;

namespace quiz.Repo.Interface;

public interface IUserQuizAttemptRepository
{
    Task<Userquizattempt?> GetAttemptByUserAndQuizAsync(int userId, int quizId, int categoryId);
    Task<int> CreateAttemptAsync(Userquizattempt attempt);

    Task UpdateAttemptAsync(Userquizattempt attempt);

     Task<IEnumerable<ActiveQuiz>> GetActiveQuizzesAsync(); // Fetch active quizzes that are not yet submitted
    Task<Userquizattempt> GetAttemptByIdAsync(int attemptId); // Fetch a specific quiz attempt by ID
    Task<Userquizattempt?> GetAttemptByUserAndQuizAsync(int userId, int quizId);
}
