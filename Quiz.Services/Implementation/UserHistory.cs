using Microsoft.Extensions.Logging;
using quiz.Domain.Dto;
using quiz.Repo.Interface;
using Quiz.Services.Interface;

namespace Quiz.Services.Implementation;

public class UserHistory : IUserHistory
{
    private readonly IQuizRepository _quizRepository;
    private readonly ILogger<UserHistory> _logger;

    private readonly IUserQuizAttemptRepository _attemptRepo;
    public UserHistory(IQuizRepository quizRepository, ILogger<UserHistory> logger, IUserQuizAttemptRepository attemptRepo)
    {
        _quizRepository = quizRepository;
        _logger = logger;
        _attemptRepo = attemptRepo;
    }

    public async Task<List<UserQuizHistoryDto>> GetUserQuizHistoryAsync(int userId)
    {
        var userQuizAttempts = await _quizRepository.GetUserQuizAttemptsAsync(userId);

        return userQuizAttempts.Select(attempt => new UserQuizHistoryDto
        {
            AttemptId = attempt.Id,
            QuizId = attempt.Quizid,
            QuizTitle = attempt.Quiz.Title,
            Score = attempt.Score,
            TimeSpent = attempt.Timespent,
            StartedAt = attempt.Startedat,
            EndedAt = attempt.Endedat,
            IsSubmitted = attempt.Issubmitted,
            CategoryId = attempt.Categoryid,
            CategoryName = attempt.Category?.Name,
            UserAnswers = attempt.Useranswers.Select(answer => new UserAnswerDto
            {
                QuestionId = answer.Questionid,
                QuestionText = answer.Question.Text,
                OptionId = answer.Optionid,
                OptionText = answer.Option.Text,
                IsCorrect = answer.Iscorrect
            }).ToList()
        }).ToList();
    }


    public async Task<bool> isUserExistAsync(int userId)
    {
        return await _attemptRepo.IsUserExistAsync(userId);
    }

    public async Task<ValidationResult> ValidateUserIdAsync(int userId)
    {
        if (userId <= 0)
            return ValidationResult.Failure("Invalid user ID.");

        var isUserValid = await isUserExistAsync(userId);
        if (!isUserValid)
            return ValidationResult.Failure("User does not exist.");

        var quizHistory = await _quizRepository.GetUserQuizAttemptsAsync(userId);
        if (quizHistory == null || !quizHistory.Any())
            return ValidationResult.Failure("No quiz history found for the user.");

        return ValidationResult.Success();
    }


}
