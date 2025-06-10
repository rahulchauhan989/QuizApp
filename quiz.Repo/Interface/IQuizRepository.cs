using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;

namespace quiz.Repo.Interface;

public interface IQuizRepository
{
    Task<Quiz> CreateQuizAsync(Quiz quiz);
    Task<IEnumerable<Question>> GetRandomQuestionsAsync(int Categoryid, int count);

    Task<Question> CreateQuestionAsync(Question question);

    Task<int> GetQuestionCountByCategoryAsync(int Categoryid);

    Task<bool> IsCategoryExistsAsync(int Categoryid);

    Task<bool> IsQuizTitleExistsAsync(string title, int quizId);

    Task<IEnumerable<QuizListDto>> GetFilteredQuizzesAsync(QuizFilterDto filter);

    // Task<Userquizattempt> SubmitQuizAttemptAsync(SubmitQuizAttemptDto dto);

    Task<List<CorrectAnswerDto>> GetCorrectAnswersForQuizAsync(int categoryId);

    Task<int> GetTotalMarksByQuizIdAsync(SubmitQuizRequest request);

    Task<int> GetQuetionsMarkByIdAsync(int questionId);

    Task<Quiz> GetQuizByIdAsync(int quizId);


}
