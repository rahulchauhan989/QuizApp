using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quiz.Domain.Dto;
using Quiz.Services.Implementation;
using Quiz.Services.Interface;

namespace WebApiQuiz.Controllers;

[ApiController]
[Route("api/quiz-submission")]
public class QuizSubmissionController : ControllerBase
{
    private readonly IQuizesSubmission _quizesSubmissionService;
    private readonly ILogger<QuizSubmissionController> _logger;

    // private readonly QuizSubmissionScheduler _quizSubmissionScheduler;

    public QuizSubmissionController(ILogger<QuizSubmissionController> logger, IQuizesSubmission quizesSubmissionService
        )
    {
        _quizesSubmissionService = quizesSubmissionService;
        _logger = logger;
        // _quizSubmissionScheduler = quizSubmissionScheduler;
    }

    [HttpPost("search")]
    [Authorize(Roles = "Admin, User")]
    public async Task<ActionResult<ResponseDto>> SearchQuizzes([FromBody] QuizFilterDto filter)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var validationResult = await _quizesSubmissionService.ValidateQuizFilterAsync(filter);
            if (!validationResult.IsValid)
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);

            var quizzes = await _quizesSubmissionService.GetFilteredQuizzesAsync(filter);
            if (quizzes == null || !quizzes.Any())
                return new ResponseDto(false, "No quizzes found matching the criteria.", null, 404);

            return new ResponseDto(true, "Quizzes fetched successfully.", quizzes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching quizzes.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpGet("{quizId}")]
    [Authorize(Roles = "Admin, User")]
    public async Task<ActionResult<ResponseDto>> GetQuizWithQuestions(int quizId)
    {
        if (quizId <= 0)
            return BadRequest("Invalid quiz ID.");
        try
        {
            var quiz = await _quizesSubmissionService.GetQuizByIdAsync(quizId);
            if (quiz == null)
                return new ResponseDto(false, "Quiz not found.", null, 404);

            var questions = await _quizesSubmissionService.GetQuestionsForQuizAsync(quizId);
            return new ResponseDto(true, "Quiz details fetched successfully.", new
            {
                Quiz = quiz,
                Questions = questions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching quiz details.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpPost("start")]
    [Authorize]
    public async Task<ActionResult<ResponseDto>> StartQuiz([FromBody] StartQuizRequest request)
    {
        try
        {
            var validationResult = await _quizesSubmissionService.ValidateQuizStartAsync(request);
            if (!validationResult.IsValid)
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);

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

            //  _quizSubmissionScheduler.ScheduleQuizSubmission(attemptId, DateTime.UtcNow, quiz.Durationminutes ?? 0);

            return new ResponseDto(true, "Quiz started successfully.", new
            {
                AttemptId = attemptId,
                Quiz = quiz,
                Questions = questions,
                Response = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting quiz.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpPost("submit")]
    [Authorize]
    public async Task<ActionResult<ResponseDto>> SubmitQuiz([FromBody] SubmitQuizRequest request)
    {
        try
        {
            var validationResult = await _quizesSubmissionService.ValidateQuizSubmissionAsync(request);
            if (!validationResult.IsValid)
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);

            var score = await _quizesSubmissionService.SubmitQuizAsync(request);
            if (score == -1000)
                return new ResponseDto(false, "Quiz submission failed. There was a existingAttempt.", null, 500);

            return new ResponseDto(true, "Quiz submitted successfully.", new
            {
                Score = score
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while submitting quiz.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

}