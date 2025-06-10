
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost("create-quiz")]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
    {
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
            return StatusCode(500, "An internal server error occurred: ");
        }
    }

    [HttpGet("{categoryId}/random-questions/{count}")]
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

    [HttpPost("create-question")]
    public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto dto)
    {
        try
        {
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

    [HttpPost("list")]
    public async Task<IActionResult> GetQuizzes([FromBody] QuizFilterDto filter)
    {
        try
        {
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

    // [HttpPost("submit-attempt")]
    // public async Task<IActionResult> SubmitAttempt([FromBody] SubmitQuizAttemptDto dto)
    // {
    //     if (dto == null || dto.Answers == null || dto.Answers.Count == 0)
    //         return BadRequest("Invalid attempt data.");

    //     var result = await _quizService.SubmitQuizAttemptAsync(dto);
    //     return Ok(result); // returns Userquizattempt
    // }

    [HttpPost("submit")]
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

            if (quiz.Enddate > request.EndedAt)
                return BadRequest($"Quiz has ended on {quiz.Enddate}. You cannot submit answers after this.");

            if (quiz.Durationminutes.HasValue &&
               (request.EndedAt - request.StartedAt).TotalMinutes > quiz.Durationminutes.Value)
            {
                return BadRequest($"Quiz duration exceeded. Allowed duration is {quiz.Durationminutes} minutes.");
            }

            if (quiz.Startdate.HasValue &&
               request.StartedAt < quiz.Startdate.Value)
            {
                return BadRequest($"Quiz has not started yet. Start date is {quiz.Startdate}.");
            }

            var score = await _quizService.SubmitQuizAsync(request);
            return Ok(new { Message = "Quiz submitted successfully", Score = score });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

}