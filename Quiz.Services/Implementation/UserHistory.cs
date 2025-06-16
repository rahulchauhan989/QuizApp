using Microsoft.Extensions.Logging;
using quiz.Domain.ViewModels;
using quiz.Repo.Interface;
using Quiz.Services.Interface;

namespace Quiz.Services.Implementation;

public class UserHistory : IUserHistory
{
    private readonly IQuizRepository _quizRepository;
    private readonly ILogger<UserHistory> _logger;
    public UserHistory(IQuizRepository quizRepository, ILogger<UserHistory> logger)
    {
        _quizRepository = quizRepository;
        _logger = logger;
    }

    #region User Quiz History

    public async Task<List<UserQuizHistoryDto>> GetUserQuizHistoryAsync(int userId)
    {
        // Fetch user quiz attempts from the repository
        var userQuizAttempts = await _quizRepository.GetUserQuizAttemptsAsync(userId);

        // Map the data to the DTO
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

    #endregion

}
