// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using quiz.Domain.ViewModels;
// using Quiz.Services.Interface;

// namespace WebApiQuiz.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class QuizController : ControllerBase
// {
//     private readonly IQuizService _quizService;
//     private readonly ILogger<QuizController> _logger;

//     public QuizController(IQuizService quizService, ILogger<QuizController> logger)
//     {
//         _quizService = quizService;
//         _logger = logger;
//     }

//     #region Quiz Management

//     [HttpPost("create-quiz")]
//     [Authorize(Roles = "Admin")] // Only Admins can create quizzes
//     public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
//     {
//         if (!ModelState.IsValid)
//             return BadRequest(ModelState);
//         try
//         {
//             var validationResult = await _quizService.ValidateQuizAsync(dto);
//             if (!validationResult.IsValid)
//                 return BadRequest(validationResult.ErrorMessage);

//             var createdQuiz = await _quizService.CreateQuizAsync(dto);
//             return Ok(createdQuiz); // Returns Quiz DTO
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error occurred while creating quiz.");
//             return StatusCode(500, "An internal server error occurred.");
//         }
//     }

//     [HttpPost("create-from-existing-questions")]
//     [Authorize(Roles = "Admin")] // Only Admins can create quizzes from existing questions
//     public async Task<IActionResult> CreateQuizFromExistingQuestions([FromBody] CreateQuizFromExistingQuestionsDto dto)
//     {
//         try
//         {
//             if (dto.QuestionIds == null || !dto.QuestionIds.Any())
//                 return BadRequest("At least one question must be selected.");

//             var validatingResult = await _quizService.ValidateQuizFromExistingQuestions(dto);

//             if (!validatingResult.IsValid)
//                 return BadRequest(validatingResult.ErrorMessage);

//             var createdQuiz = await _quizService.CreateQuizFromExistingQuestionsAsync(dto);
//             return Ok(createdQuiz); // Returns Quiz DTO
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error occurred while creating quiz from existing questions.");
//             return StatusCode(500, "An internal server error occurred.");
//         }
//     }

//     #endregion

//     #region Quiz Submission

//     [HttpPost("submit")]
//     [Authorize] // Only authenticated users can submit quizzes
//     public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizRequest request)
//     {
//         try
//         {
//             var Totalmarks = await _quizService.GetTotalMarksAsync(request);

//             int Inputmarks = 0;

//             foreach (var answer in request.Answers)
//             {
//                 if (answer.OptionId <= 0 || answer.QuestionId <= 0)
//                     return BadRequest("Invalid answer data.");
//             }

//             foreach (var answer in request.Answers)
//             {
//                 int QuetionsMark = await _quizService.GetQuetionsMarkByIdAsync(answer.QuestionId);
//                 Inputmarks += QuetionsMark;
//             }

//             if (Inputmarks > Totalmarks)
//                 return BadRequest($"Total marks {Inputmarks} exceed the quiz total marks {Totalmarks}.");

//             CreateQuizViewModel quiz = await _quizService.GetQuizByIdAsync(request.QuizId);

//             if (quiz == null)
//                 return NotFound($"Quiz with ID {request.QuizId} does not exist.");

//             if (quiz.Durationminutes.HasValue &&
//                (request.EndedAt - request.StartedAt).TotalMinutes > quiz.Durationminutes.Value)
//             {
//                 return BadRequest($"Quiz duration exceeded. Allowed duration is {quiz.Durationminutes} minutes.");
//             }

//             var score = await _quizService.SubmitQuizAsync(request);
//             return Ok(new { Message = "Quiz submitted successfully", Score = score });
//         }
//         catch (Exception ex)
//         {
//             return BadRequest(new { ex.Message });
//         }
//     }

//     #endregion

//     #region User Quiz History

//     [HttpGet("user/{userId}/quiz-history")]
//     [Authorize] // Only authenticated users can view their quiz history
//     public async Task<IActionResult> GetUserQuizHistory(int userId)
//     {
//         try
//         {
//             var quizHistory = await _quizService.GetUserQuizHistoryAsync(userId);

//             if (quizHistory == null || !quizHistory.Any())
//                 return NotFound($"No quiz history found for user with ID {userId}.");

//             return Ok(quizHistory);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error occurred while fetching user quiz history.");
//             return StatusCode(500, "An internal server error occurred.");
//         }
//     }

//     #endregion
// }