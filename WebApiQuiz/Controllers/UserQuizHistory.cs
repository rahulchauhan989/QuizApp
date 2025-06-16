using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz.Services.Interface;

namespace WebApiQuiz.Controllers;

public class UserQuizHistory : ControllerBase
{
    private readonly IUserHistory _userHistoryService;
    private readonly ILogger<UserQuizHistory> _logger;
    public UserQuizHistory(IUserHistory userHistoryService, ILogger<UserQuizHistory> logger)
    {
        _userHistoryService = userHistoryService;
        _logger = logger;
    }


    #region User Quiz History

    [HttpGet("user/{userId}/quiz-history")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserQuizHistory(int userId)
    {
        try
        {
            var quizHistory = await _userHistoryService.GetUserQuizHistoryAsync(userId);

            if (quizHistory == null || !quizHistory.Any())
                return NotFound($"No quiz history found for user with ID {userId}.");

            return Ok(quizHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user quiz history.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    #endregion
}
