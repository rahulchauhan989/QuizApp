using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quiz.Domain.ViewModels;
using Quiz.Services.Interface;

namespace WebApiQuiz.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly ILogger<QuizController> _logger;

    private readonly IQuestionServices _questionService;

    public QuizController(IQuizService quizService, ILogger<QuizController> logger, IQuestionServices questionService)
    {
        _questionService = questionService;
        _quizService = quizService;
        _logger = logger;
    }


    [HttpPost("create-quiz-only")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQuizOnly([FromBody] CreateQuizOnlyDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            bool isCategoryExists = await _questionService.IsCategoryExistsAsync(dto.Categoryid);
            if (!isCategoryExists)
                return BadRequest("Category does not exist.");

            var result = await _quizService.CreateQuizOnlyAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create quiz.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPost("add-questions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddQuestionsToQuiz([FromBody] AddQuestionToQuizDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            if (dto.ExistingQuestionIds != null && dto.ExistingQuestionIds.Any() && dto.ExistingQuestionIds.All(id => id > 0))
            {
                var validateResults = await _quizService.ValidateQuizForExistingQuestions(dto);
                if (!validateResults.IsValid)
                    return BadRequest(validateResults.ErrorMessage);

                var validateResult = await _quizService.validateQuiz(dto);
                if (!validateResult.IsValid)
                    return BadRequest(validateResult.ErrorMessage);

                var addedQuestions = await _quizService.AddExistingQuestionsToQuizAsync(dto.QuizId, dto.ExistingQuestionIds);
                return Ok(new { Message = "Questions added to quiz successfully.", Questions = addedQuestions });
            }

            if (!string.IsNullOrWhiteSpace(dto.Text) && dto.Marks.HasValue && !string.IsNullOrWhiteSpace(dto.Difficulty) && dto.Options != null)
            {
                var validateResults = await _quizService.ValidateQuizAsyncForNewQuestions(dto);
                if (!validateResults.IsValid)
                    return BadRequest(validateResults.ErrorMessage);
                var newQuestion = await _quizService.AddNewQuestionToQuizAsync(dto);
                return Ok(new { Message = "New question added to quiz successfully.", Question = newQuestion });
            }

            return BadRequest("Invalid request. Either provide existing question IDs or complete details for a new question.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add questions to quiz.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPost("remove-question")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveQuestionFromQuiz([FromBody] RemoveQuestionFromQuizDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _quizService.RemoveQuestionFromQuizAsyncValidation(dto.QuizId, dto.QuestionId);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            bool result = await _quizService.RemoveQuestionFromQuizAsync(dto);

            return Ok("Question removed from quiz successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove question from quiz.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("{id}/edit")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetQuizForEdit(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Invalid Quiz ID.");

            var quiz = await _quizService.GetQuizForEditAsync(id);
            return quiz != null ? Ok(quiz) : NotFound("Quiz not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching quiz for edit.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPut("edit-quiz")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditQuiz([FromBody] UpdateQuizDto dto)
    {
        try
        {
            var updatedQuiz = await _quizService.UpdateQuizAsync(dto);
            if (updatedQuiz == null)
                return NotFound("Quiz not found or already inactive.");
            return Ok(updatedQuiz);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quiz.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPut("{id}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PublishQuiz(int id)
    {
        try
        {
            var validationResult = await _quizService.PublishQuizAsync(id);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            return Ok(new { Message = "Quiz published successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while publishing quiz.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPut("{id}/unpublish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnpublishQuiz(int id)
    {
        try
        {
            var validationResult = await _quizService.UnpublishQuizAsync(id);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            return Ok(new { Message = "Quiz unpublished successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while unpublishing quiz.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpDelete("delete-quiz/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        try
        {
            var validationResult = await _quizService.validateForDeleteQuiz(id);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var deleted = await _quizService.SoftDeleteQuizAsync(id);
            return Ok(new { message = "Quiz deleted (soft delete) successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quiz.");
            return StatusCode(500, "Internal server error.");
        }
    }



// if we want to Create Quiz with Questions simultaneously otherwise Above Api 
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
            return Ok(createdQuiz);
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
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
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


}