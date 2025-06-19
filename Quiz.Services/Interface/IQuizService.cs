using quiz.Domain.DataModels;
using quiz.Domain.Dto;

namespace Quiz.Services.Interface;

public interface IQuizService
{
    Task<QuizDto> CreateQuizAsync(CreateQuizDto dto);
    Task<ValidationResult> ValidateQuizAsync(CreateQuizDto dto);
    Task<QuizDto> CreateQuizFromExistingQuestionsAsync(CreateQuizFromExistingQuestionsDto dto);
    Task<ValidationResult> ValidateQuizFromExistingQuestions(CreateQuizFromExistingQuestionsDto dto);
    Task<IEnumerable<ActiveQuiz>> GetActiveQuizzesAsync();
    Task SubmitQuizAutomaticallyAsync(int attemptId);
    Task<QuizDto> CreateQuizOnlyAsync(CreateQuizOnlyDto dto);
    Task<ValidationResult> ValidateQuizForExistingQuestions(AddQuestionToQuizDto dto);
    Task<ValidationResult> ValidateQuizAsyncForNewQuestions(AddQuestionToQuizDto dto);
    Task<ValidationResult> validateQuiz(AddQuestionToQuizDto dto);
    Task<List<QuestionDto>> AddExistingQuestionsToQuizAsync(int quizId, List<int> existingQuestionIds);
    Task<QuestionDto> AddNewQuestionToQuizAsync(AddQuestionToQuizDto dto);
    Task<QuizEditDto?> GetQuizForEditAsync(int quizId);
    Task<QuizDto?> UpdateQuizAsync(UpdateQuizDto dto);
    Task<bool> SoftDeleteQuizAsync(int id);
    Task<bool> RemoveQuestionFromQuizAsync(RemoveQuestionFromQuizDto dto);
    Task<ValidationResult> PublishQuizAsync(int quizId);
    Task<ValidationResult> RemoveQuestionFromQuizAsyncValidation(int quizId, int questionId);
    Task<ValidationResult> validateForEditQuiz(QuizEditDto dto);
    Task<ValidationResult> validateForDeleteQuiz(int quizId);
    Task<ValidationResult> UnpublishQuizAsync(int quizId);
    Task<ValidationResult> ValidateUpdateQuizAsync(UpdateQuizDto dto);
}
