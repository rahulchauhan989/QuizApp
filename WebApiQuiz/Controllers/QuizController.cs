using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quiz.Domain.Dto;
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
    public async Task<ActionResult<ResponseDto>> CreateQuizOnly([FromBody] CreateQuizOnlyDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            bool isCategoryExists = await _questionService.IsCategoryExistsAsync(dto.Categoryid);
            if (!isCategoryExists)
                return new ResponseDto(false, "Category does not exist.", null, 404);

            var result = await _quizService.CreateQuizOnlyAsync(dto);
            return Ok(new ResponseDto(true, "Quiz created successfully.", result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create quiz.");
            return new ResponseDto(false, "Internal server error.", null, 500);
        }
    }

    [HttpPost("add-questions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> AddQuestionsToQuiz([FromBody] AddQuestionToQuizDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            if (dto.ExistingQuestionIds != null && dto.ExistingQuestionIds.Any() && dto.ExistingQuestionIds.All(id => id > 0))
            {
                var validateResults = await _quizService.ValidateQuizForExistingQuestions(dto);
                
                if (!validateResults.IsValid)
                    return new ResponseDto(false, validateResults.ErrorMessage, null, 400);

                var validateResult = await _quizService.validateQuiz(dto);
                if (!validateResult.IsValid)
                    return new ResponseDto(false, validateResult.ErrorMessage, null, 400);

                var addedQuestions = await _quizService.AddExistingQuestionsToQuizAsync(dto.QuizId, dto.ExistingQuestionIds);
                return new ResponseDto(true, "Questions added to quiz successfully.", addedQuestions);
            }

            if (!string.IsNullOrWhiteSpace(dto.Text) && dto.Marks.HasValue && !string.IsNullOrWhiteSpace(dto.Difficulty) && dto.Options != null)
            {
                var validateResults = await _quizService.ValidateQuizAsyncForNewQuestions(dto);

                if (!validateResults.IsValid)
                    return new ResponseDto(false, validateResults.ErrorMessage, null, 400);

                var newQuestion = await _quizService.AddNewQuestionToQuizAsync(dto);
                return new ResponseDto(true, "New question added to quiz successfully.", newQuestion);
            }

            return new ResponseDto(false, "Invalid data provided for adding questions to quiz.", null, 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add questions to quiz.");
            return new ResponseDto(false, "Internal server error.", null, 500);
        }
    }

    [HttpPost("remove-question")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> RemoveQuestionFromQuiz([FromBody] RemoveQuestionFromQuizDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _quizService.RemoveQuestionFromQuizAsyncValidation(dto.QuizId, dto.QuestionId);
            if (!validationResult.IsValid)
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);

            bool result = await _quizService.RemoveQuestionFromQuizAsync(dto);

            return result 
                ? new ResponseDto(true, "Question removed from quiz successfully.") 
                : new ResponseDto(false, "Failed to remove question from quiz or question not found in the quiz.", null, 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove question from quiz.");
            return new ResponseDto(false, "Internal server error.", null, 500);
        }
    }

    [HttpGet("{id}/edit")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> GetQuizForEdit(int id)
    {
        try
        {
            if (id <= 0)
                return new ResponseDto(false, "Invalid Quiz ID.", null, 400);

            var quiz = await _quizService.GetQuizForEditAsync(id);
            return quiz != null ? 
                new ResponseDto(true, "Quiz fetched successfully.", quiz) 
                : new ResponseDto(false, "Quiz not found or already inactive.", null, 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching quiz for edit.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpPut("edit-quiz")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> EditQuiz([FromBody] UpdateQuizDto dto)
    {
        try
        {
            var updatedQuiz = await _quizService.UpdateQuizAsync(dto);
            if (updatedQuiz == null)
                return new ResponseDto(false, "Quiz not found or already inactive.", null, 404);
            return Ok(new ResponseDto(true, "Quiz updated successfully.", updatedQuiz));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quiz.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpPut("{id}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> PublishQuiz(int id)
    {
        try
        {
            var validationResult = await _quizService.PublishQuizAsync(id);
            if (!validationResult.IsValid)
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);

            return new ResponseDto(true, "Quiz published successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while publishing quiz.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpPut("{id}/unpublish")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> UnpublishQuiz(int id)
    {
        try
        {
            var validationResult = await _quizService.UnpublishQuizAsync(id);
            if (!validationResult.IsValid)
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);

            return new ResponseDto(true, "Quiz unpublished successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while unpublishing quiz.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpDelete("delete-quiz/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> DeleteQuiz(int id)
    {
        try
        {
            var validationResult = await _quizService.validateForDeleteQuiz(id);
            if (!validationResult.IsValid)
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);

            var deleted = await _quizService.SoftDeleteQuizAsync(id);
            return deleted 
                ? new ResponseDto(true, "Quiz deleted successfully.", null) 
                : new ResponseDto(false, "Failed to delete quiz or quiz not found.", null, 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quiz.");
            return StatusCode(500, "Internal server error.");
        }
    }


    [HttpPost("create-quiz")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> CreateQuiz([FromBody] CreateQuizDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _quizService.ValidateQuizAsync(dto);
            if (!validationResult.IsValid)
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);

            var createdQuiz = await _quizService.CreateQuizAsync(dto);
            return new ResponseDto(true, "Quiz created successfully.", createdQuiz);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating quiz.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpPost("create-from-existing-questions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> CreateQuizFromExistingQuestions([FromBody] CreateQuizFromExistingQuestionsDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validatingResult = await _quizService.ValidateQuizFromExistingQuestions(dto);

            if (!validatingResult.IsValid)
                return new ResponseDto(false, validatingResult.ErrorMessage, null, 400);

            var createdQuiz = await _quizService.CreateQuizFromExistingQuestionsAsync(dto);
            return new ResponseDto(true, "Quiz created from existing questions successfully.", createdQuiz);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating quiz from existing questions.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }


}