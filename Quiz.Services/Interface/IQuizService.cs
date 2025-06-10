using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;

namespace Quiz.Services.Interface;

public interface IQuizService
{
    // Task<QuizDto> CreateQuizAsync(quiz.Domain.DataModels.Quiz quiz);

    Task<QuizDto> CreateQuizAsync(CreateQuizDto dto);
    // Task<IEnumerable<Question>> GetRandomQuestionsAsync(int quizId, int count);

    Task<List<QuestionDto>> GetRandomQuestionsAsync(int Categoryid, int count);

    Task<QuestionDto> CreateQuestionAsync(QuestionCreateDto dto);

    Task<int> GetQuestionCountByCategoryAsync(int Categoryid);

    Task<bool> IsCategoryExistsAsync(int Categoryid);

    Task<bool> IsQuizTitleExistsAsync(string title, int quizId);

    Task<ValidationResult> ValidateQuizAsync(CreateQuizDto dto);

    Task<ValidationResult> ValidateQuestionAsync(QuestionCreateDto dto);

    Task<IEnumerable<QuizListDto>> GetFilteredQuizzesAsync(QuizFilterDto filter);

    Task<ValidationResult> ValidateQuizFilterAsync(QuizFilterDto filter);

    // Task<Userquizattempt> SubmitQuizAttemptAsync(SubmitQuizAttemptDto dto);

    Task<int> SubmitQuizAsync(SubmitQuizRequest request);

    Task<int> GetTotalMarksAsync(SubmitQuizRequest request);

    Task<int> GetQuetionsMarkByIdAsync(int questionId);

    Task<CreateQuizViewModel> GetQuizByIdAsync(int quizId);
}
