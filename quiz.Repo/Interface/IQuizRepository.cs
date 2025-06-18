using quiz.Domain.DataModels;
using quiz.Domain.Dto;

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

    Task<int> GetQuestionCountByCategoryAsync(int Categoryid);

    Task<bool> IsCategoryExistsAsync(int Categoryid);

    Task<bool> IsQuizTitleExistsAsync(string title);

    Task<IEnumerable<QuizListDto>> GetFilteredQuizzesAsync(QuizFilterDto filter);

    Task<List<CorrectAnswerDto>> GetCorrectAnswersForQuizAsync(int categoryId);

    Task<int> GetTotalMarksByQuizIdAsync(SubmitQuizRequest request);

    Task<int> GetQuetionsMarkByIdAsync(int questionId);

    Task<Quiz> GetQuizByIdAsync(int quizId);

    Task<IEnumerable<Question>> GetQuestionsByIdsAsync(List<int> questionIds);
    Task AddQuestionToQuiz(Quiz quiz, Question question);
    Task<List<Userquizattempt>> GetUserQuizAttemptsAsync(int userId);

    Task<Question?> GetQuestionByIdAsync(int questionId);
    Task LinkExistingQuestionToQuizAsync(int quizId, int questionId);

    Task UpdateQuizAsync(Quiz quiz);
    Task SoftDeleteQuizAsync(int id);

    Task<bool> RemoveQuestionFromQuizAsync(int quizId, int questionId);

    //Question 

    Task UpdateQuestionAsync(Question question);
    Task RemoveOptionsByQuestionIdAsync(int questionId);

    Task<int> GetQuizQuestionsMarksSumAsync(int quizId);

    Task<bool> IsQuestionUsedInAnswersAsync(int questionId);
    Task<bool> IsQuestionInPublicQuizAsync(int questionId);

    // Task<Quiz?> GetQuizByIdAsync(int quizId);
    Task<bool> IsQuestionInQuizAsync(int quizId, int questionId);
    // Task RemoveQuestionFromQuizAsync(int quizId, int questionId);
    Task<bool> HasUnsubmittedAttemptsAsync(int quizId);


}
