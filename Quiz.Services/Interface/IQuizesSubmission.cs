using quiz.Domain.Dto;

namespace Quiz.Services.Interface;

public interface IQuizesSubmission
{
    Task<CreateQuizzDto> GetQuizByIdAsync(int quizId);
    Task<bool> CheckExistingAttemptAsync(int userId, int quizId, int categoryId);
    Task<IEnumerable<QuestionDto>> GetQuestionsForQuizAsync(int quizId);
    Task<int> StartQuizAsync(int userId, int quizId, int categoryId);
    Task<int> GetTotalMarksAsync(SubmitQuizRequest request);
    Task<int> GetQuetionsMarkByIdAsync(int questionId);
    Task<int> SubmitQuizAsync(SubmitQuizRequest request);
    Task<ValidationResult> ValidateQuizFilterAsync(QuizFilterDto filter);
    Task<IEnumerable<QuizListDto>> GetFilteredQuizzesAsync(QuizFilterDto filter);
    Task<ValidationResult> ValidateQuizSubmissionAsync(SubmitQuizRequest request);
    Task<ValidationResult> ValidateQuizStartAsync(StartQuizRequest request);

}
