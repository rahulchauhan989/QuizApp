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

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    // [HttpPost("create-quiz")]
    // public async Task<IActionResult> CreateQuiz([FromBody] quiz.Domain.DataModels.Quiz quiz)
    // {
    //     var created = await _quizService.CreateQuizAsync(quiz);
    //     return Ok(created); //returns DTO
    // }


    [HttpPost("create-quiz")]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
    {
        string[] difficulty = { "Easy", "Medium", "Hard" };
        try
        {
            if (dto == null)
                return BadRequest("Quiz data is required.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Quiz title cannot be empty.");

            if (dto.Totalmarks <= 0)
                return BadRequest("Total marks must be greater than zero.");

            if (dto.Durationminutes <= 0)
                return BadRequest("Duration must be greater than zero.");

            if (dto.Startdate >= dto.Enddate)
                return BadRequest("Start date must be before end date.");

            if (dto.Enddate - dto.Startdate != TimeSpan.FromMinutes(dto.Durationminutes!.Value))
                return BadRequest("Duration does not match the difference between start and end date.");

            if (dto.Categoryid <= 0)
                return BadRequest("Invalid Category ID.");

            if (dto.Createdby <= 0)
                return BadRequest("Invalid Creator ID.");

            var isTitleExists = await _quizService.IsQuizTitleExistsAsync(dto.Title, dto.Categoryid);

            if (isTitleExists)
                return BadRequest($"Quiz with title '{dto.Title}' already exists in category {dto.Categoryid}.");

            // Validate questions if provided
            if (dto.Questions != null && dto.Questions.Any())
            {
                foreach (var question in dto.Questions)
                {
                    if (string.IsNullOrWhiteSpace(question.Text))
                        return BadRequest("Question text cannot be empty.");

                    if (question.Marks <= 0)
                        return BadRequest("Question marks must be greater than zero.");

                    if (string.IsNullOrWhiteSpace(question.Difficulty))
                        return BadRequest("Question difficulty cannot be empty.");

                    if (question.Options == null || question.Options.Count != 4)
                        return BadRequest("Each question must have Four options.");

                    if (!question.Options.Any(o => o.IsCorrect))
                        return BadRequest("At least one option must be marked as correct.");

                    if (question.Options.Count(o => o.IsCorrect) > 1)
                        return BadRequest("Only one option can be marked as correct.");

                    if (!difficulty.Contains(question.Difficulty))
                        return BadRequest($"Invalid difficulty level: {question.Difficulty}");

                }
            }
            var created = await _quizService.CreateQuizAsync(dto);
            return Ok(created); // returns Quiz Dto 
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("{Categoryid}/random-questions/{count}")]
    public async Task<IActionResult> GetRandomQuestions(int Categoryid, int count)
    {
        try
        {
            if (Categoryid <= 0)
                return BadRequest("Invalid Category ID.");

            bool categoryExists = await _quizService.IsCategoryExistsAsync(Categoryid);

            if (!categoryExists)
                return NotFound($"Category with ID {Categoryid} does not exist.");

            if (count <= 0)
                return BadRequest("Count must be greater than zero.");

            var existingQuetionCount = await _quizService.GetQuestionCountByCategoryAsync(Categoryid);

            if (existingQuetionCount < count)
                return BadRequest($"Not enough questions available in category {Categoryid}. Available: {existingQuetionCount}, Requested: {count}");

            var questions = await _quizService.GetRandomQuestionsAsync(Categoryid, count);
            return Ok(questions); //  returns List<QuestionDto>
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred: {ex.Message}");
        }

    }

    [HttpPost("create-question")]
    public async Task<IActionResult> Create([FromBody] QuestionCreateDto dto)
    {
        string[] difficulty = { "Easy", "Medium", "Hard" };
        try
        {
            if (dto == null)
                return BadRequest("Question data is required.");

            if (string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest("Question text cannot be empty.");

            if (dto.QuizId <= 0)
                return BadRequest("Invalid Quiz ID.");

            if (dto.Categoryid <= 0)
                return BadRequest("Invalid category ID.");

            if (dto.Options == null || dto.Options.Count != 4)
                return BadRequest("Each question must have Four options.");

            if (!dto.Options.Any(o => o.IsCorrect))
                return BadRequest("At least one option must be marked as correct.");

            if (dto.Options.Any(o => string.IsNullOrWhiteSpace(o.Text)))
                return BadRequest("Option text cannot be empty.");

            var totalcorrectCount = dto.Options.Count(o => o.IsCorrect);
            if (totalcorrectCount > 1)
                return BadRequest("Only one option can be marked as correct.");

            if (dto.Marks <= 0)
                return BadRequest("Marks must be greater than zero.");

            if (string.IsNullOrWhiteSpace(dto.Difficulty))
                return BadRequest("Difficulty level cannot be empty.");

            if (!difficulty.Contains(dto.Difficulty))
                return BadRequest($"Invalid difficulty level: {dto.Difficulty}");

            var created = await _quizService.CreateQuestionAsync(dto);
            return Ok(created); // This is now a DTO, safe to serialize
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred: {ex.Message}");
        }
    }
}

