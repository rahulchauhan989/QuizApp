using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quiz.Domain.Dto;
using Quiz.Services.Interface;

namespace WebApiQuiz.Controllers;

[ApiController]
[Route("api/quiz-submission")]
public class QuizSubmissionController : ControllerBase
{
    private readonly IQuizesSubmission _quizesSubmissionService;
    private readonly ILogger<QuizSubmissionController> _logger;

    public QuizSubmissionController(IQuizesSubmission quizesSubmissionService, ILogger<QuizSubmissionController> logger)
    {
        _quizesSubmissionService = quizesSubmissionService;
        _logger = logger;
    }

    [HttpPost("search")]
    [Authorize(Roles = "Admin, User")]
    public async Task<IActionResult> SearchQuizzes([FromBody] QuizFilterDto filter)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var validationResult = await _quizesSubmissionService.ValidateQuizFilterAsync(filter);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var quizzes = await _quizesSubmissionService.GetFilteredQuizzesAsync(filter);
            return Ok(quizzes); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching quizzes.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("{quizId}")]
    [Authorize(Roles = "Admin, User")]
    public async Task<IActionResult> GetQuizWithQuestions(int quizId)
    {
        if (quizId <= 0)
            return BadRequest("Invalid quiz ID.");
        try
        {
            var quiz = await _quizesSubmissionService.GetQuizByIdAsync(quizId);
            if (quiz == null)
                return NotFound("Quiz not found.");

            var questions = await _quizesSubmissionService.GetQuestionsForQuizAsync(quizId);
            return Ok(new { Quiz = quiz, Questions = questions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching quiz details.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost("start")]
    [Authorize]
    public async Task<IActionResult> StartQuiz([FromBody] StartQuizRequest request)
    {
        try
        {
            var validationResult = await _quizesSubmissionService.ValidateQuizStartAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var quiz = await _quizesSubmissionService.GetQuizByIdAsync(request.QuizId);
            var questions = await _quizesSubmissionService.GetQuestionsForQuizAsync(request.QuizId);

            var attemptId = await _quizesSubmissionService.StartQuizAsync(request.UserId, request.QuizId, request.categoryId);

            var response = new SubmitQuizRequest
            {
                UserId = request.UserId,
                QuizId = request.QuizId,
                categoryId = request.categoryId,
                Answers = questions.Select(q => new SubmittedAnswer
                {
                    QuestionId = q.Id,
                    OptionId = q.Options!.FirstOrDefault()?.Id ?? 0,
                }).ToList(),
                StartedAt = DateTime.UtcNow,
                EndedAt = DateTime.UtcNow.AddMinutes(quiz.Durationminutes ?? 0)
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while starting quiz.");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting quiz.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost("submit")]
    [Authorize]
    public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizRequest request)
    {
        try
        {
            var validationResult = await _quizesSubmissionService.ValidateQuizSubmissionAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var score = await _quizesSubmissionService.SubmitQuizAsync(request);
            if (score == -1000)
                return BadRequest("Quiz already submitted or no score available.");

            return Ok(new { Message = "Quiz submitted successfully", Score = score });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while submitting quiz.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

}