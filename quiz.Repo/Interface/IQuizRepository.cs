using quiz.Domain.DataModels;

namespace quiz.Repo.Interface;

public interface IQuizRepository
{
    Task<Quiz> CreateQuizAsync(Quiz quiz);
    Task<IEnumerable<Question>> GetRandomQuestionsAsync(int Categoryid, int count);

    Task<Question> CreateQuestionAsync(Question question);

    Task<int> GetQuestionCountByCategoryAsync(int Categoryid);

    Task<bool> IsCategoryExistsAsync(int Categoryid);

    Task<bool> IsQuizTitleExistsAsync(string title, int quizId);
}
