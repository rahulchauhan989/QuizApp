using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;

namespace quiz.Repo.Interface;

public interface IQuizRepository
{
    Task<Quiz> CreateQuizAsync(Quiz quiz);

    Task AddQuestionToQuizAsync(Quiz quiz, Question question);
    Task<IEnumerable<Question>> GetRandomQuestionsAsync(int Categoryid, int count);

    Task<bool> IsQuizExistsAsync(int quizId);

    Task<IEnumerable<Question>> GetRandomQuestionsByQuizIdAsync(int quizId, int count);

    Task<IEnumerable<Question>> GetQuestionsByQuizIdAsync(int quizId);

    Task<int> GetQuestionCountByQuizIdAsync(int quizId);

    Task<Question> CreateQuestionAsync(Question question);

    // Task AddQuestionToQuizAsync(int quizId, int questionId);

    Task<int> GetQuestionCountByCategoryAsync(int Categoryid);

    Task<bool> IsCategoryExistsAsync(int Categoryid);

    Task<bool> IsQuizTitleExistsAsync(string title);

    Task<IEnumerable<QuizListDto>> GetFilteredQuizzesAsync(QuizFilterDto filter);

    // Task<Userquizattempt> SubmitQuizAttemptAsync(SubmitQuizAttemptDto dto);

    Task<List<CorrectAnswerDto>> GetCorrectAnswersForQuizAsync(int categoryId);

    Task<int> GetTotalMarksByQuizIdAsync(SubmitQuizRequest request);

    Task<int> GetQuetionsMarkByIdAsync(int questionId);

    Task<Quiz> GetQuizByIdAsync(int quizId);

    Task<IEnumerable<Question>> GetQuestionsByIdsAsync(List<int> questionIds);
    Task AddQuestionToQuiz(Quiz quiz, Question question);
    Task<List<Userquizattempt>> GetUserQuizAttemptsAsync(int userId);

}
