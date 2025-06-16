

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
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (dto == null)
                return BadRequest("Question data is required.");

            var validationResult = await _questionService.ValidateQuestionAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);


            var createdQuestion = await _questionService.CreateQuestionAsync(dto);
            return Ok(createdQuestion); // Returns Question DTO
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
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
    [Authorize(Roles = "Admin, User")] // Allow both Admins and Users to fetch random questions
    public async Task<IActionResult> GetRandomQuestions(int categoryId, int count)
    {
        try
        {
            if (categoryId <= 0)
                return BadRequest("Invalid Category ID.");

            if (count <= 0)
                return BadRequest("Count must be greater than zero.");

            var categoryExists = await _questionService.IsCategoryExistsAsync(categoryId);
            if (!categoryExists)
                return NotFound($"Category with ID {categoryId} does not exist.");

            var availableQuestions = await _questionService.GetQuestionCountByCategoryAsync(categoryId);
            if (availableQuestions < count)
                return BadRequest($"Not enough questions available in category {categoryId}. Available: {availableQuestions}, Requested: {count}");

            var questions = await _questionService.GetRandomQuestionsAsync(categoryId, count);
            return Ok(questions); // Returns List<QuestionDto>
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching random questions.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("quiz/{quizId}/random/{count}")]
    [Authorize(Roles = "Admin, User")] // Allow both Admins and Users to fetch random questions by quiz ID
    public async Task<IActionResult> GetRandomQuestionByQuizId(int quizId, int count)
    {
        try
        {
            if (quizId <= 0)
                return BadRequest("Invalid Quiz ID.");

            if (count <= 0)
                return BadRequest("Count must be greater than zero.");

            var quizExists = await _questionService.IsQuizExistsAsync(quizId);
            if (!quizExists)
                return NotFound($"Quiz with ID {quizId} does not exist.");

            var availableQuestions = await _questionService.GetQuestionCountByQuizIdAsync(quizId);
            if (availableQuestions < count)
                return BadRequest($"Not enough questions available in quiz {quizId}. Available: {availableQuestions}, Requested: {count}");

            var questions = await _questionService.GetRandomQuestionsByQuizIdAsync(quizId, count);
            return Ok(questions); // Returns List<QuestionDto>
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching random questions.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }
}
