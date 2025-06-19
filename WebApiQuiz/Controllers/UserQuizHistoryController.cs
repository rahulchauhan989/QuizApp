using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quiz.Domain.Dto;
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

    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> GetUserQuizHistory(int userId)
    {
        try
        {
            var validationResult = await _userHistoryService.ValidateUserIdAsync(userId);
            if (!validationResult.IsValid)
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);

            var quizHistory = await _userHistoryService.GetUserQuizHistoryAsync(userId);
            return quizHistory != null && quizHistory.Any() 
                ? new ResponseDto(true, "User quiz history fetched successfully.", quizHistory)
                : new ResponseDto(false, "No quiz history found for the user.", null, 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user quiz history.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }
}