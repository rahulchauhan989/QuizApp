using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quiz.Domain.ViewModels;
using Quiz.Services.Interface;

namespace WebApiQuiz.Controllers;

public class QuizSubmissionController : ControllerBase
{
    private readonly IQuizesSubmission _quizesSubmissionService;
    private readonly ILogger<QuizSubmissionController> _logger;



    public QuizSubmissionController(IQuizesSubmission quizesSubmissionService, ILogger<QuizSubmissionController> logger)
    {
        _quizesSubmissionService = quizesSubmissionService;
        _logger = logger;
    }

    #region Quiz Submission


    [HttpPost("SearchQuizzes")]
    [Authorize(Roles = "Admin, User")]
    public async Task<IActionResult> GetQuizzes([FromBody] QuizFilterDto filter)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (filter == null)
                return BadRequest("Filter data is required.");

            var validationResult = await _quizesSubmissionService.ValidateQuizFilterAsync(filter);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var quizzes = await _quizesSubmissionService.GetFilteredQuizzesAsync(filter);
            return Ok(quizzes); // List<QuizListDto>
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating quiz filter.");
            return StatusCode(500, "An internal server error occurred during validation.");
        }
    }



    [HttpPost("start-quiz")]
    [Authorize] // Only authenticated users can start a quiz
    public async Task<IActionResult> StartQuiz([FromBody] StartQuizRequest request)
    {
        try
        {
            // Validate quiz ID
            var quiz = await _quizesSubmissionService.GetQuizByIdAsync(request.QuizId);
            if (quiz == null || quiz.Isdeleted == true)
                return NotFound($"Quiz with ID {request.QuizId} does not exist or is deleted.");

            // Check if quiz is public or active
            if (quiz.Ispublic != true)
                return BadRequest("This quiz is not public or active.");

            // Check for existing attempts
            bool existingAttempt = await _quizesSubmissionService.CheckExistingAttemptAsync(request.UserId, request.QuizId, request.categoryId);
            if (existingAttempt)
                return BadRequest("You have already started this quiz or May be Submitted Also");

            // Fetch questions for the quiz
            var questions = await _quizesSubmissionService.GetQuestionsForQuizAsync(request.QuizId);
            if (questions == null || !questions.Any())
                return NotFound("No questions found for this quiz.");

            // Start the quiz
            var attemptId = await _quizesSubmissionService.StartQuizAsync(request.UserId, request.QuizId, request.categoryId);

            // Populate the SubmitQuizRequest view model
            var response = new SubmitQuizRequest
            {
                UserId = request.UserId,
                QuizId = request.QuizId,
                categoryId = request.categoryId,
                Answers = questions.Select(q => new SubmittedAnswer
                {
                    QuestionId = q.Id,
                    OptionId = 0
                }).ToList(),
                StartedAt = DateTime.UtcNow,
                EndedAt = DateTime.UtcNow.AddMinutes(quiz.Durationminutes ?? 0)
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting quiz.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost("submit")]
    [Authorize] // Only authenticated users can submit quizzes
    public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizRequest request)
    {
        try
        {
            var Totalmarks = await _quizesSubmissionService.GetTotalMarksAsync(request);

            int Inputmarks = 0;

            foreach (var answer in request.Answers)
            {
                if (answer.OptionId <= 0 || answer.QuestionId <= 0)
                    return BadRequest("Invalid answer data.");
            }

            foreach (var answer in request.Answers)
            {
                int QuetionsMark = await _quizesSubmissionService.GetQuetionsMarkByIdAsync(answer.QuestionId);
                Inputmarks += QuetionsMark;
            }

            if (Inputmarks > Totalmarks)
                return BadRequest($"Total marks {Inputmarks} exceed the quiz total marks {Totalmarks}.");

            CreateQuizViewModel quiz = await _quizesSubmissionService.GetQuizByIdAsync(request.QuizId);

            if (quiz == null)
                return NotFound($"Quiz with ID {request.QuizId} does not exist.");

            if (quiz.Durationminutes.HasValue &&
               (request.EndedAt - request.StartedAt).TotalMinutes > quiz.Durationminutes.Value)
            {
                return BadRequest($"Quiz duration exceeded. Allowed duration is {quiz.Durationminutes} minutes.");
            }

            var score = await _quizesSubmissionService.SubmitQuizAsync(request);
            return Ok(new { Message = "Quiz submitted successfully", Score = score });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    #endregion


}
