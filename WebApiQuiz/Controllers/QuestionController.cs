using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quiz.Domain.ViewModels;
using Quiz.Services.Interface;

namespace WebApiQuiz.Controllers;

[Route("api/questions")]
[ApiController]
public class QuestionController : ControllerBase
{
    private readonly IQuestionServices _questionService;
    private readonly ILogger<QuestionController> _logger;

    public QuestionController(IQuestionServices questionService, ILogger<QuestionController> logger)
    {
        _questionService = questionService;
        _logger = logger;
    }

    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _questionService.ValidateQuestionAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var createdQuestion = await _questionService.CreateQuestionAsync(dto);
            return Ok(createdQuestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating question.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("{id}/edit")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetQuestionForEdit(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Invalid Question ID.");

            var question = await _questionService.GetQuestionForEditAsync(id);
            return question != null ? Ok(question) : NotFound("Question not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching question for edit.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPut("{id}/update")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditQuestion([FromBody] QuestionUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _questionService.ValidateQuestionUpdate(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var result = await _questionService.EditQuestionAsync(dto);
            return result != null ? Ok(result) : NotFound("Question not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update question.");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpDelete("{id}/delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SoftDeleteQuestion(int id)
    {
        try
        {
            var validationResult = await _questionService.validateDeleteQuestionAsync(id);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            bool result = await _questionService.SoftDeleteQuestionAsync(id);
            return result ? Ok("Question deleted successfully.") : NotFound("Question not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete question.");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("category/{categoryId}/random/{count}")]
    [Authorize(Roles = "Admin, User")]
    public async Task<IActionResult> GetRandomQuestions(int categoryId, int count)
    {
        try
        {
            var validationResult = await _questionService.ValidateGetRandomQuestionsAsync(categoryId, count);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var questions = await _questionService.GetRandomQuestionsAsync(categoryId, count);
            return Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching random questions.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("quiz/{quizId}/random/{count}")]
    [Authorize(Roles = "Admin, User")]
    public async Task<IActionResult> GetRandomQuestionByQuizId(int quizId, int count)
    {
        try
        {
            var validationResult = await _questionService.ValidateGetRandomQuestionsByQuizIdAsync(quizId, count);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var questions = await _questionService.GetRandomQuestionsByQuizIdAsync(quizId, count);
            return Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching random questions.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }
}
