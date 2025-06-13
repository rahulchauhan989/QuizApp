using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using quiz.Domain.ViewModels;
using Quiz.Services.Interface;

namespace WebApiQuiz.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly ILogger<QuizController> _logger;

    public QuizController(IQuizService quizService, ILogger<QuizController> logger)
    {
        _quizService = quizService;
        _logger = logger;
    }

    #region Quiz Management

    [HttpPost("create-quiz")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _quizService.ValidateQuizAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var createdQuiz = await _quizService.CreateQuizAsync(dto);
            return Ok(createdQuiz); // Returns Quiz DTO
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating quiz.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost("create-from-existing-questions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQuizFromExistingQuestions([FromBody] CreateQuizFromExistingQuestionsDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (dto.QuestionIds == null || !dto.QuestionIds.Any())
                return BadRequest("At least one question must be selected.");

            var validatingResult = await _quizService.ValidateQuizFromExistingQuestions(dto);

            if (!validatingResult.IsValid)
                return BadRequest(validatingResult.ErrorMessage);

            var createdQuiz = await _quizService.CreateQuizFromExistingQuestionsAsync(dto);
            return Ok(createdQuiz); // Returns Quiz DTO
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating quiz from existing questions.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost("list")]
    [Authorize(Roles = "Admin, User")] // Allow both Admins and Users to view quizzes
    public async Task<IActionResult> GetQuizzes([FromBody] QuizFilterDto filter)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (filter == null)
                return BadRequest("Filter data is required.");

            var validationResult = await _quizService.ValidateQuizFilterAsync(filter);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var quizzes = await _quizService.GetFilteredQuizzesAsync(filter);
            return Ok(quizzes); // List<QuizListDto>
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating quiz filter.");
            return StatusCode(500, "An internal server error occurred during validation.");
        }
    }

    #endregion

    #region Question Management

    [HttpPost("create-question")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (dto == null)
                return BadRequest("Question data is required.");

            var validationResult = await _quizService.ValidateQuestionAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var createdQuestion = await _quizService.CreateQuestionAsync(dto);
            return Ok(createdQuestion); // Returns Question DTO
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating question.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("{categoryId}/random-questions/{count}")]
    [Authorize(Roles = "Admin, User")] // Allow both Admins and Users to fetch random questions
    public async Task<IActionResult> GetRandomQuestions(int categoryId, int count)
    {
        try
        {
            if (categoryId <= 0)
                return BadRequest("Invalid Category ID.");

            if (count <= 0)
                return BadRequest("Count must be greater than zero.");

            var categoryExists = await _quizService.IsCategoryExistsAsync(categoryId);
            if (!categoryExists)
                return NotFound($"Category with ID {categoryId} does not exist.");

            var availableQuestions = await _quizService.GetQuestionCountByCategoryAsync(categoryId);
            if (availableQuestions < count)
                return BadRequest($"Not enough questions available in category {categoryId}. Available: {availableQuestions}, Requested: {count}");

            var questions = await _quizService.GetRandomQuestionsAsync(categoryId, count);
            return Ok(questions); // Returns List<QuestionDto>
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching random questions.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("{quizId}/random-question/{count}")]
    [Authorize(Roles = "Admin, User")] // Allow both Admins and Users to fetch random questions by quiz ID
    public async Task<IActionResult> GetRandomQuestionByQuizId(int quizId, int count)
    {
        try
        {
            if (quizId <= 0)
                return BadRequest("Invalid Quiz ID.");

            if (count <= 0)
                return BadRequest("Count must be greater than zero.");

            var quizExists = await _quizService.IsQuizExistsAsync(quizId);
            if (!quizExists)
                return NotFound($"Quiz with ID {quizId} does not exist.");

            var availableQuestions = await _quizService.GetQuestionCountByQuizIdAsync(quizId);
            if (availableQuestions < count)
                return BadRequest($"Not enough questions available in quiz {quizId}. Available: {availableQuestions}, Requested: {count}");

            var questions = await _quizService.GetRandomQuestionsByQuizIdAsync(quizId, count);
            return Ok(questions); // Returns List<QuestionDto>
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching random questions.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }


    #endregion

    #region Quiz Submission


    [HttpPost("start-quiz")]
    [Authorize] // Only authenticated users can start a quiz
    public async Task<IActionResult> StartQuiz([FromBody] StartQuizRequest request)
    {
        try
        {
            // Validate quiz ID
            var quiz = await _quizService.GetQuizByIdAsync(request.QuizId);
            if (quiz == null || quiz.Isdeleted == true)
                return NotFound($"Quiz with ID {request.QuizId} does not exist or is deleted.");

            // Check if quiz is public or active
            if (quiz.Ispublic != true)
                return BadRequest("This quiz is not public or active.");

            // Check for existing attempts
            bool existingAttempt = await _quizService.CheckExistingAttemptAsync(request.UserId, request.QuizId, request.categoryId);
            if (existingAttempt)
                return BadRequest("You have already started this quiz or May be Submitted Also");

            // Fetch questions for the quiz
            var questions = await _quizService.GetQuestionsForQuizAsync(request.QuizId);
            if (questions == null || !questions.Any())
                return NotFound("No questions found for this quiz.");

            // Start the quiz
            var attemptId = await _quizService.StartQuizAsync(request.UserId, request.QuizId, request.categoryId);

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
            var Totalmarks = await _quizService.GetTotalMarksAsync(request);

            int Inputmarks = 0;

            foreach (var answer in request.Answers)
            {
                if (answer.OptionId <= 0 || answer.QuestionId <= 0)
                    return BadRequest("Invalid answer data.");
            }

            foreach (var answer in request.Answers)
            {
                int QuetionsMark = await _quizService.GetQuetionsMarkByIdAsync(answer.QuestionId);
                Inputmarks += QuetionsMark;
            }

            if (Inputmarks > Totalmarks)
                return BadRequest($"Total marks {Inputmarks} exceed the quiz total marks {Totalmarks}.");

            CreateQuizViewModel quiz = await _quizService.GetQuizByIdAsync(request.QuizId);

            if (quiz == null)
                return NotFound($"Quiz with ID {request.QuizId} does not exist.");

            if (quiz.Durationminutes.HasValue &&
               (request.EndedAt - request.StartedAt).TotalMinutes > quiz.Durationminutes.Value)
            {
                return BadRequest($"Quiz duration exceeded. Allowed duration is {quiz.Durationminutes} minutes.");
            }

            var score = await _quizService.SubmitQuizAsync(request);
            return Ok(new { Message = "Quiz submitted successfully", Score = score });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    #endregion

    #region User Quiz History

    [HttpGet("user/{userId}/quiz-history")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserQuizHistory(int userId)
    {
        try
        {
            var quizHistory = await _quizService.GetUserQuizHistoryAsync(userId);

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