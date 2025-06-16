using System.Net.Http.Json;
using System.Text.Json;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;
using quiz.Repo.Interface;
using Quiz.Services.Interface;

namespace Quiz.Services.Implementation;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepository;

    private readonly ILoginService _loginService;

    private readonly IUserQuizAttemptRepository _attemptRepo;

    private readonly IUserAnswerRepository _answerRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public QuizService(
        IQuizRepository quizRepository,
        IUserQuizAttemptRepository attemptRepo,
        IUserAnswerRepository answerRepo,
        IHttpContextAccessor httpContextAccessor,
        ILoginService loginService)
    {
        _quizRepository = quizRepository ?? throw new ArgumentNullException(nameof(quizRepository));
        _attemptRepo = attemptRepo ?? throw new ArgumentNullException(nameof(attemptRepo));
        _answerRepo = answerRepo ?? throw new ArgumentNullException(nameof(answerRepo));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
    }


    #region Quiz Management
    public async Task<ValidationResult> ValidateQuizAsync(CreateQuizDto dto)
    {
        return await Task.Run(async () =>
        {
            string[] difficultyLevels = { "Easy", "Medium", "Hard", "easy", "medium", "hard" };

            if (dto == null)
                return ValidationResult.Failure("Quiz data is required.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return ValidationResult.Failure("Quiz title cannot be empty.");

            if (dto.Totalmarks <= 0)
                return ValidationResult.Failure("Total marks must be greater than zero.");

            if (dto.Durationminutes <= 0)
                return ValidationResult.Failure("Duration must be greater than zero.");

            if (dto.Categoryid <= 0)
                return ValidationResult.Failure("Invalid Category ID.");

            bool isValidCategory = _quizRepository.IsCategoryExistsAsync(dto.Categoryid).Result;
            if (!isValidCategory)
                return ValidationResult.Failure("Category does not exist.");

            bool titleExists = await _quizRepository.IsQuizTitleExistsAsync(dto.Title);
            if (titleExists)
                return ValidationResult.Failure("Quiz title already exists.");

            // Validate questions if provided
            if (dto.Questions != null && dto.Questions.Any())
            {
                int totalQuestionMarks = dto.Questions.Sum(q => q.Marks);
                if (totalQuestionMarks != dto.Totalmarks)
                    return ValidationResult.Failure($"Total marks of the quiz: {dto.Totalmarks} must match the sum of the marks of its questions.{totalQuestionMarks}");

                foreach (var question in dto.Questions)
                {
                    if (string.IsNullOrWhiteSpace(question.Text))
                        return ValidationResult.Failure("Question text cannot be empty.");

                    if (question.Marks <= 0)
                        return ValidationResult.Failure("Question marks must be greater than zero.");

                    if (string.IsNullOrWhiteSpace(question.Difficulty) || !difficultyLevels.Contains(question.Difficulty))
                        return ValidationResult.Failure($"Invalid difficulty level: {question.Difficulty}");

                    if (question.Options == null || question.Options.Count != 4)
                        return ValidationResult.Failure("Each question must have four options.");

                    if (!question.Options.Any(o => o.IsCorrect))
                        return ValidationResult.Failure("At least one option must be marked as correct.");

                    if (question.Options.Count(o => o.IsCorrect) > 1)
                        return ValidationResult.Failure("Only one option can be marked as correct.");
                }
            }
            return ValidationResult.Success();
        });
    }

    public async Task<QuizDto> CreateQuizOnlyAsync(CreateQuizOnlyDto dto)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "")!;
        var userId = _loginService.ExtractUserIdFromToken(token);

        var quiz = new quiz.Domain.DataModels.Quiz
        {
            Title = dto.Title!,
            Description = dto.Description,
            Totalmarks = dto.Totalmarks,
            Durationminutes = dto.Durationminutes,
            Ispublic = dto.Ispublic,
            Categoryid = dto.Categoryid,
            Createdby = userId
        };

        await _quizRepository.CreateQuizAsync(quiz);

        return new QuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            Totalmarks = quiz.Totalmarks,
            Durationminutes = quiz.Durationminutes ?? 0,
            Ispublic = quiz.Ispublic,
            Categoryid = quiz.Categoryid,
            Createdby = quiz.Createdby
        };
    }

    public async Task<List<QuestionDto>> AddExistingQuestionsToQuizAsync(int quizId, List<int> existingQuestionIds)
    {
        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
        if (quiz == null)
            throw new Exception("Quiz not found.");

        var existingQuestions = await _quizRepository.GetQuestionsByIdsAsync(existingQuestionIds);
        if (!existingQuestions.Any())
            throw new Exception("No valid questions found.");

        foreach (var question in existingQuestions)
        {
            await _quizRepository.LinkExistingQuestionToQuizAsync(quizId, question.Id);
        }

        return existingQuestions.Select(q => new QuestionDto
        {
            Id = q.Id,
            Text = q.Text,
            Marks = q.Marks,
            Difficulty = q.Difficulty,
            Options = q.Options.Select(o => new OptionDto
            {
                Id = o.Id,
                Text = o.Text,
                IsCorrect = o.Iscorrect
            }).ToList()
        }).ToList();
    }

    public async Task<ValidationResult> ValidateQuizForExistingQuestions(AddQuestionToQuizDto dto)
    {
        if (dto == null)
            return ValidationResult.Failure("Quiz data is required.");

        var quiz = await _quizRepository.GetQuizByIdAsync(dto.QuizId);
        if (quiz == null)
            return ValidationResult.Failure("Quiz not found.");

        var existingQuestions = await _quizRepository.GetQuestionsByIdsAsync(dto.ExistingQuestionIds!);
        if (!existingQuestions.Any())
            return ValidationResult.Failure("No valid questions found for the provided IDs.");

        int totalMarks = existingQuestions.Sum(q => q.Marks) ?? 0;
        int QuizQuestionMarkSum = await _quizRepository.GetQuizQuestionsMarksSumAsync(dto.QuizId);
        int QuizTotalMarks = quiz.Totalmarks;

        if (totalMarks + QuizQuestionMarkSum > QuizTotalMarks)
            return ValidationResult.Failure($"Total marks of the Existing Question ({totalMarks})+ QuizQuestionMarkSum ({QuizQuestionMarkSum})   must be less than Totak Quiz Marks ({QuizTotalMarks}).");

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateQuizAsyncForNewQuestions(AddQuestionToQuizDto dto)
    {
        if (dto == null)
            return ValidationResult.Failure("Quiz data is required.");

        if (string.IsNullOrWhiteSpace(dto.Text))
            return ValidationResult.Failure("Question text cannot be empty.");

        if (!dto.Marks.HasValue || dto.Marks <= 0)
            return ValidationResult.Failure("Question marks must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.Difficulty) || !new[] { "Easy", "Medium", "Hard" }.Contains(dto.Difficulty))
            return ValidationResult.Failure($"Invalid difficulty level: {dto.Difficulty}");

        if (dto.Options == null || dto.Options.Count != 4)
            return ValidationResult.Failure("Each question must have four options.");

        if (!dto.Options.Any(o => o.IsCorrect))
            return ValidationResult.Failure("At least one option must be marked as correct.");

        if (dto.Options.Count(o => o.IsCorrect) > 1)
            return ValidationResult.Failure("Only one option can be marked as correct.");

        var quiz = await _quizRepository.GetQuizByIdAsync(dto.QuizId);
        if (quiz == null)
            return ValidationResult.Failure("Quiz not found.");

        int quizTotalMarks = quiz.Totalmarks;
        if (quizTotalMarks <= 0)
            return ValidationResult.Failure("Total marks of the quiz must be greater than zero.");

        // Fetch the sum of marks of all questions associated with the quiz
        int quizQuestionsMarksSum = await _quizRepository.GetQuizQuestionsMarksSumAsync(dto.QuizId);

        // Check if the sum of marks exceeds the total marks of the quiz
        if (quizQuestionsMarksSum + dto.Marks.Value > quizTotalMarks)
            return ValidationResult.Failure($"The sum of marks of the questions in the quiz ({quizQuestionsMarksSum}) + the marks of the newly added question ({dto.Marks.Value}) exceeds the total marks of the quiz ({quizTotalMarks}).");

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> validateQuiz(AddQuestionToQuizDto dto)
    {
        var quiz = await _quizRepository.GetQuizByIdAsync(dto.QuizId);
        if (quiz == null)
            return ValidationResult.Failure("Quiz not found.");

        var existingQuestions = await _quizRepository.GetQuestionsByIdsAsync(dto.ExistingQuestionIds!);
        if (!existingQuestions.Any())
            return ValidationResult.Failure("No valid questions found for the provided IDs.");

        // Validate that all existing questions belong to the same category as the quiz
        foreach (var question in existingQuestions)
        {
            if (question.CategoryId != quiz.Categoryid)
                return ValidationResult.Failure($"Question ID {question.Id} does not belong to the same category as the quiz.");
        }

        return ValidationResult.Success();
    }

    public async Task<QuestionDto> AddNewQuestionToQuizAsync(AddQuestionToQuizDto dto)
    {
        string token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "")!;
        int userId = _loginService.ExtractUserIdFromToken(token);

        var quiz = await _quizRepository.GetQuizByIdAsync(dto.QuizId);
        if (quiz == null)
            throw new Exception("Quiz not found.");

        var question = new Question
        {
            Text = dto.Text!,
            Marks = dto.Marks!.Value,
            Difficulty = dto.Difficulty,
            CategoryId = quiz.Categoryid,
            Modifierdat = DateTime.UtcNow.ToLocalTime(),
            Updatedby = userId,
            Options = dto.Options?.Select(o => new Option
            {
                Text = o.Text!,
                Iscorrect = o.IsCorrect
            }).ToList() ?? new List<Option>()
        };

        await _quizRepository.AddQuestionToQuizAsync(quiz, question);

        return new QuestionDto
        {
            Id = question.Id,
            Text = question.Text,
            Marks = question.Marks,
            Difficulty = question.Difficulty,
            Options = question.Options.Select(o => new OptionDto
            {
                Id = o.Id,
                Text = o.Text,
                IsCorrect = o.Iscorrect
            }).ToList()
        };
    }

    public async Task<QuizDto> CreateQuizAsync(CreateQuizDto dto)
    {
        string token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "")!;

        int userId = _loginService.ExtractUserIdFromToken(token);

        var quiz = new quiz.Domain.DataModels.Quiz
        {
            Title = dto.Title!,
            Description = dto.Description,
            Totalmarks = dto.Totalmarks,
            Durationminutes = dto.Durationminutes,
            Ispublic = dto.Ispublic,
            Categoryid = dto.Categoryid,
            // Createdby = dto.Createdby,
            Createdby = userId,
        };

        // Save the quiz to generate its ID
        await _quizRepository.CreateQuizAsync(quiz);

        // Handle questions and associate them with the quiz
        if (dto.Questions != null && dto.Questions.Any())
        {
            foreach (var questionDto in dto.Questions)
            {
                var question = new Question
                {
                    Text = questionDto.Text!,
                    Marks = questionDto.Marks,
                    Difficulty = questionDto.Difficulty,
                    CategoryId = dto.Categoryid, // Assuming all questions belong to the same category
                    Options = questionDto.Options?.Select(o => new Option
                    {
                        Text = o.Text!,
                        Iscorrect = o.IsCorrect,
                    }).ToList() ?? new List<Option>()
                };

                // Add the question to the database and associate it with the quiz
                await _quizRepository.AddQuestionToQuizAsync(quiz, question);
            }
        }

        // Fetch the questions associated with the quiz
        var questions = await _quizRepository.GetQuestionsByQuizIdAsync(quiz.Id);

        // Return the output DTO
        return new QuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title!,
            Description = quiz.Description,
            Totalmarks = quiz.Totalmarks,
            Durationminutes = quiz.Durationminutes,
            Ispublic = quiz.Ispublic,
            Categoryid = quiz.Categoryid,
            Createdby = quiz.Createdby,
            Questions = questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Marks = q.Marks,
                Difficulty = q.Difficulty,
                Options = q.Options.Select(o => new OptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.Iscorrect
                }).ToList()
            }).ToList()
        };
    }

    public async Task<ValidationResult> ValidateQuizFromExistingQuestions(CreateQuizFromExistingQuestionsDto dto)
    {
        if (dto == null)
            return ValidationResult.Failure("Quiz data is required.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return ValidationResult.Failure("Quiz title cannot be empty.");

        if (dto.Totalmarks <= 0)
            return ValidationResult.Failure("Total marks must be greater than zero.");

        if (dto.Durationminutes <= 0)
            return ValidationResult.Failure("Duration must be greater than zero.");

        if (dto.Categoryid <= 0)
            return ValidationResult.Failure("Invalid Category ID.");

        var selectedQuestions = await _quizRepository.GetQuestionsByIdsAsync(dto.QuestionIds);
        int totalQuestionMarks = selectedQuestions.Sum(q => q.Marks) ?? 0;
        if (totalQuestionMarks != dto.Totalmarks)
        {
            return ValidationResult.Failure("Total marks of the quiz must match the sum of the marks of the selected questions.");
        }

        return ValidationResult.Success();

    }

    public async Task<QuizDto> CreateQuizFromExistingQuestionsAsync(CreateQuizFromExistingQuestionsDto dto)
    {
        // Validate total marks of selected questions
        var selectedQuestions = await _quizRepository.GetQuestionsByIdsAsync(dto.QuestionIds);

        var quiz = new quiz.Domain.DataModels.Quiz
        {
            Title = dto.Title,
            Description = dto.Description,
            Totalmarks = dto.Totalmarks,
            Durationminutes = dto.Durationminutes,
            Ispublic = dto.Ispublic,
            Categoryid = dto.Categoryid,
            Createdby = dto.Createdby,
        };

        // Save the quiz to generate its ID
        await _quizRepository.CreateQuizAsync(quiz);

        // Associate the selected questions with the quiz
        foreach (var question in selectedQuestions)
        {
            await _quizRepository.AddQuestionToQuiz(quiz, question);
        }

        // Return the output DTO
        return new QuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            Totalmarks = quiz.Totalmarks,
            Durationminutes = quiz.Durationminutes,
            Ispublic = quiz.Ispublic,
            Categoryid = quiz.Categoryid,
            Createdby = quiz.Createdby,
            Questions = selectedQuestions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Marks = q.Marks,
                Difficulty = q.Difficulty,
                Options = q.Options.Select(o => new OptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.Iscorrect
                }).ToList()
            }).ToList()
        };
    }

    public async Task<ValidationResult> PublishQuizAsync(int quizId)
    {
        // Fetch the quiz
        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
        if (quiz == null)
            return ValidationResult.Failure("Quiz not found.");

        // Fetch the sum of marks of all questions associated with the quiz
        int quizQuestionsMarksSum = await _quizRepository.GetQuizQuestionsMarksSumAsync(quizId);

        // Validate that the total marks of the quiz match the sum of the marks of its questions
        if (quiz.Totalmarks != quizQuestionsMarksSum)
            return ValidationResult.Failure($"Quiz total marks ({quiz.Totalmarks}) must match the sum of the marks of its questions ({quizQuestionsMarksSum}).");

        // Publish the quiz by setting Ispublic to true
        quiz.Ispublic = true;
        quiz.Modifiedat = DateTime.UtcNow.ToLocalTime();

        await _quizRepository.UpdateQuizAsync(quiz);

        return ValidationResult.Success();
    }

    #endregion

    public async Task<QuizEditDto?> GetQuizForEditAsync(int quizId)
    {
        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
        if (quiz == null || quiz.Isdeleted == true)
            return null;

        var questions = await _quizRepository.GetQuestionsByQuizIdAsync(quizId);

        return new QuizEditDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            Totalmarks = quiz.Totalmarks,
            Durationminutes = (int)quiz.Durationminutes!,
            Ispublic = (bool)quiz.Ispublic!,
            Categoryid = quiz.Categoryid,
            Questions = questions.Select(q => new QuestionEditDto
            {
                Id = q.Id,
                Text = q.Text,
                Marks = q.Marks ?? 0,
                Difficulty = q.Difficulty!,
                Options = q.Options.Select(o => new OptionEditDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.Iscorrect
                }).ToList()
            }).ToList()
        };
    }

    public async Task<QuizDto?> UpdateQuizAsync(UpdateQuizDto dto)
    {
        string token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "")!;
        int userId = _loginService.ExtractUserIdFromToken(token);

        var quiz = await _quizRepository.GetQuizByIdAsync(dto.Id);
        if (quiz == null || quiz.Isdeleted == true)
            return null;

        quiz.Title = dto.Title ?? quiz.Title;
        quiz.Description = dto.Description ?? quiz.Description;
        quiz.Totalmarks = dto.Totalmarks ?? quiz.Totalmarks;
        quiz.Durationminutes = dto.Durationminutes ?? quiz.Durationminutes;
        quiz.Ispublic = dto.Ispublic ?? quiz.Ispublic;
        quiz.Categoryid = dto.Categoryid ?? quiz.Categoryid;
        quiz.Modifiedat = DateTime.UtcNow.ToLocalTime();
        quiz.Modifiedby = userId;

        await _quizRepository.UpdateQuizAsync(quiz);

        return new QuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            Totalmarks = quiz.Totalmarks,
            Durationminutes = quiz.Durationminutes,
            Ispublic = quiz.Ispublic,
            Categoryid = quiz.Categoryid,
            Createdby = quiz.Createdby
        };
    }

    public async Task<bool> SoftDeleteQuizAsync(int id)
    {
        var quiz = await _quizRepository.GetQuizByIdAsync(id);
        if (quiz == null || quiz.Isdeleted == true)
            return false;

        await _quizRepository.SoftDeleteQuizAsync(id);
        return true;
    }

    public async Task<bool> RemoveQuestionFromQuizAsync(RemoveQuestionFromQuizDto dto)
    {
        return await _quizRepository.RemoveQuestionFromQuizAsync(dto.QuizId, dto.QuestionId);
    }


    #region Automatic Quiz Submission
    public async Task<IEnumerable<ActiveQuiz>> GetActiveQuizzesAsync()
    {
        // Fetch active quizzes that are not yet submitted
        return await _attemptRepo.GetActiveQuizzesAsync();
    }

    public async Task SubmitQuizAutomaticallyAsync(int attemptId)
    {
        // Fetch the attempt details
        var attempt = await _attemptRepo.GetAttemptByIdAsync(attemptId);
        if (attempt == null || attempt.Issubmitted == true)
            throw new Exception("Attempt not found or already submitted.");

        // Submit the quiz
        var correctAnswers = await _quizRepository.GetCorrectAnswersForQuizAsync(attempt.Categoryid ?? 0);
        int score = 0;

        foreach (var answer in attempt.Useranswers)
        {
            var correctOptionId = correctAnswers
                .FirstOrDefault(c => c.QuestionId == answer.Questionid)?.CorrectOptionId;

            if (correctOptionId == answer.Optionid)
                score++;
        }

        attempt.Score = score;
        attempt.Endedat = DateTime.UtcNow;
        if (attempt.Endedat.HasValue && attempt.Startedat.HasValue)
        {
            attempt.Timespent = (int)(attempt.Endedat.Value - attempt.Startedat.Value).TotalMinutes;
        }
        else
        {
            attempt.Timespent = 0;
        }
        attempt.Issubmitted = true;

        await _attemptRepo.UpdateAttemptAsync(attempt);
    }

    #endregion

}
