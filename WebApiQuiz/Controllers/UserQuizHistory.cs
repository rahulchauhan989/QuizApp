
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz.Services.Interface;

namespace WebApiQuiz.Controllers;

[ApiController]
[Route("api/user-quiz-history")]
public class UserQuizHistoryController : ControllerBase
{
    private readonly IUserHistory _userHistoryService;
    private readonly ILogger<UserQuizHistoryController> _logger;

    public UserQuizHistoryController(IUserHistory userHistoryService, ILogger<UserQuizHistoryController> logger)
    {
        _userHistoryService = userHistoryService;
        _logger = logger;
    }

    #region User Quiz History

    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserQuizHistory(int userId)
    {
        try
        {
            var validationResult = await _userHistoryService.ValidateUserIdAsync(userId);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var quizHistory = await _userHistoryService.GetUserQuizHistoryAsync(userId);
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