using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using quiz.Domain.DataModels;
using quiz.Domain.Dto;
using quiz.Repo.Interface;
using Quiz.Services.Interface;
using ValidationResult = quiz.Domain.Dto.ValidationResult;

namespace Quiz.Services.Implementation;

public class QuestionServices : IQuestionServices
{

    private readonly IQuizRepository _quizRepository;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILoginService _loginService;
    private readonly ILogger<QuestionServices> _logger;
    public QuestionServices(IQuizRepository quizRepository, ILogger<QuestionServices> logger, IHttpContextAccessor httpContextAccessor, ILoginService loginService)
    {
        _quizRepository = quizRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _loginService = loginService;
    }

    public async Task<ValidationResult> ValidateQuestionAsync(QuestionCreateDto dto)
    {
        return await Task.Run(async () =>
         {
             string[] difficultyLevels = {"easy", "medium", "hard" };

             if (dto == null)
                 return ValidationResult.Failure("Question data is required.");

             if (dto.Categoryid <= 0)
                 return ValidationResult.Failure("Invalid Category ID.");

             bool isCategoryExists = await _quizRepository.IsCategoryExistsAsync(dto.Categoryid);
             if (!isCategoryExists)
                 return ValidationResult.Failure("Category does not exist.");

             if (string.IsNullOrWhiteSpace(dto.Text))
                 return ValidationResult.Failure("Question text cannot be empty.");

             if (dto.Marks <= 0)
                 return ValidationResult.Failure("Marks must be greater than zero.");

             if (string.IsNullOrWhiteSpace(dto.Difficulty) || !difficultyLevels.Contains(dto.Difficulty.ToLower()))
                 return ValidationResult.Failure($"Invalid difficulty level: {dto.Difficulty}");

             if (dto.Options == null || dto.Options.Count != 4)
                 return ValidationResult.Failure("Each question must have four options.");

             if (!dto.Options.Any(o => o.IsCorrect))
                 return ValidationResult.Failure("At least one option must be marked as correct.");

             if (dto.Options.Count(o => o.IsCorrect) > 1)
                 return ValidationResult.Failure("Only one option can be marked as correct.");

             return ValidationResult.Success();
         });
    }

    public async Task<ValidationResult> ValidateQuestionUpdate(QuestionUpdateDto dto)
    {
        string[] difficultyLevels = { "easy", "medium", "hard" };

        if (dto == null)
            return ValidationResult.Failure("Question data is required.");

        if (dto.Id <= 0)
            return ValidationResult.Failure("Invalid Question ID.");

        var existingQuestion = await _quizRepository.GetQuestionByIdAsync(dto.Id);
        if (existingQuestion == null || existingQuestion.Isdeleted == true)
            return ValidationResult.Failure("Question does not exist or has been deleted.");

        bool isQuestionUsedInAnswers = await _quizRepository.IsQuestionUsedInAnswersAsync(dto.Id);
        if (isQuestionUsedInAnswers)
            return ValidationResult.Failure("This question cannot be edited because it has been answered in one or more quizzes.");

        bool isQuestionInPublicQuiz = await _quizRepository.IsQuestionInPublicQuizAsync(dto.Id);
        if (isQuestionInPublicQuiz)
            return ValidationResult.Failure("This question cannot be edited because it is part of a public quiz.");

        if (dto.Categoryid <= 0)
            return ValidationResult.Failure("Invalid Category ID.");

        bool isCategoryExists = await _quizRepository.IsCategoryExistsAsync(dto.Categoryid);
        if (!isCategoryExists)
            return ValidationResult.Failure("Category does not exist.");

        if (string.IsNullOrWhiteSpace(dto.Text))
            return ValidationResult.Failure("Question text cannot be empty.");

        if (dto.Marks <= 0)
            return ValidationResult.Failure("Marks must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.Difficulty) || !difficultyLevels.Contains(dto.Difficulty.ToLower()))
            return ValidationResult.Failure($"Invalid difficulty level: {dto.Difficulty}");

        if (dto.Options == null || dto.Options.Count != 4)
            return ValidationResult.Failure("Each question must have four options.");

        if (!dto.Options.Any(o => o.IsCorrect))
            return ValidationResult.Failure("At least one option must be marked as correct.");

        if (dto.Options.Count(o => o.IsCorrect) > 1)
            return ValidationResult.Failure("Only one option can be marked as correct.");

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateGetRandomQuestionsAsync(int categoryId, int count)
    {
        if (categoryId <= 0)
            return ValidationResult.Failure("Invalid Category ID.");

        bool isCategoryExists = await _quizRepository.IsCategoryExistsAsync(categoryId);
        if (!isCategoryExists)
            return ValidationResult.Failure("Category does not exist.");

        if (count <= 0)
            return ValidationResult.Failure("Count must be greater than zero.");

        int questionCount = await _quizRepository.GetQuestionCountByCategoryAsync(categoryId);
        if (questionCount < count)
            return ValidationResult.Failure($"Not enough questions available in category {categoryId} to fetch {count} random questions.");

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateGetRandomQuestionsByQuizIdAsync(int quizId, int count)
    {
        if (quizId <= 0)
            return ValidationResult.Failure("Invalid Quiz ID.");

        bool isQuizExists = await _quizRepository.IsQuizExistsAsync(quizId);
        if (!isQuizExists)
            return ValidationResult.Failure("Quiz does not exist.");

        if (count <= 0)
            return ValidationResult.Failure("Count must be greater than zero.");

        int questionCount = await _quizRepository.GetQuestionCountByQuizIdAsync(quizId);
        if (questionCount < count)
            return ValidationResult.Failure($"Not enough questions available in quiz {quizId} to fetch {count} random questions.");

        return ValidationResult.Success();
    }

    public async Task<QuestionDto> CreateQuestionAsync(QuestionCreateDto dto)
    {
        string token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "")!;
        int userId = _loginService.ExtractUserIdFromToken(token);
        var question = new Question
        {
            CategoryId = dto.Categoryid,
            Text = dto.Text!,
            Marks = dto.Marks,
            Difficulty = dto.Difficulty,
            Createdby = userId,
            Options = dto.Options!.Select(o => new Option
            {
                Text = o.Text!,
                Iscorrect = o.IsCorrect,
            }).ToList()
        };

        await _quizRepository.CreateQuestionAsync(question);

        return new QuestionDto
        {
            Id = question.Id,
            Categoryid = (int)question.CategoryId!,
            Text = question.Text,
            Marks = question.Marks,
            Difficulty = question.Difficulty,
            createdBy = userId,
            Options = question.Options.Select(o => new OptionDto
            {
                Id = o.Id,
                Text = o.Text,
                IsCorrect = o.Iscorrect
            }).ToList()
        };
    }

    public async Task<bool> IsCategoryExistsAsync(int Categoryid)
    {
        bool result = await _quizRepository.IsCategoryExistsAsync(Categoryid);

        return result;
    }


    public async Task<int> GetQuestionCountByCategoryAsync(int Categoryid)
    {
        return await _quizRepository.GetQuestionCountByCategoryAsync(Categoryid);
    }

    public async Task<List<QuestionDto>> GetRandomQuestionsAsync(int Categoryid, int count)
    {
        var questions = await _quizRepository.GetRandomQuestionsAsync(Categoryid, count);

        return questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            Categoryid = (int)q.CategoryId!,
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

    public async Task<int> GetQuestionCountByQuizIdAsync(int quizId)
    {
        return await _quizRepository.GetQuestionCountByQuizIdAsync(quizId);
    }

    public async Task<List<QuestionDto>> GetRandomQuestionsByQuizIdAsync(int quizId, int count)
    {
        var questions = await _quizRepository.GetRandomQuestionsByQuizIdAsync(quizId, count);

        return questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            Categoryid = q.CategoryId ?? 0,
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

    public async Task<bool> IsQuizExistsAsync(int quizId)
    {
        return await _quizRepository.IsQuizExistsAsync(quizId);
    }


    public async Task<QuestionEditDto?> GetQuestionForEditAsync(int questionId)
    {
        var question = await _quizRepository.GetQuestionByIdAsync(questionId);
        if (question == null || question.Isdeleted == true)
            return null;

        return new QuestionEditDto
        {
            Id = question.Id,
            Text = question.Text,
            Marks = question.Marks ?? 1,
            Difficulty = question.Difficulty!,
            Categoryid = question.CategoryId ?? 0,
            Options = question.Options.Select(o => new OptionEditDto
            {
                Id = o.Id,
                Text = o.Text,
                IsCorrect = o.Iscorrect
            }).ToList()
        };
    }

    public async Task<QuestionDto?> EditQuestionAsync(QuestionUpdateDto dto)
    {
        string token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "")!;
        int userId = _loginService.ExtractUserIdFromToken(token);

        var question = await _quizRepository.GetQuestionByIdAsync(dto.Id);
        if (question == null || question.Isdeleted == true)
            return null;

        question.Text = dto.Text!;
        question.Marks = dto.Marks;
        question.Difficulty = dto.Difficulty;
        question.CategoryId = dto.Categoryid;
        question.Updatedby = userId;
        question.Createdat = question.Createdat ?? DateTime.UtcNow.ToLocalTime();
        question.Modifierdat = DateTime.UtcNow.ToLocalTime();

        await _quizRepository.RemoveOptionsByQuestionIdAsync(question.Id);

        question.Options = dto.Options?.Select(o => new Option
        {
            Text = o.Text!,
            Iscorrect = o.IsCorrect
        }).ToList() ?? new List<Option>();

        await _quizRepository.UpdateQuestionAsync(question);

        return new QuestionDto
        {
            Id = question.Id,
            Text = question.Text,
            Marks = question.Marks ?? 1,
            Difficulty = question.Difficulty,
            Categoryid = question.CategoryId ?? 0,
            Options = question.Options.Select(o => new OptionDto
            {
                Id = o.Id,
                Text = o.Text,
                IsCorrect = o.Iscorrect
            }).ToList()
        };
    }

    public async Task<ValidationResult> validateDeleteQuestionAsync(int questionId)
    {
        if (questionId <= 0)
            return ValidationResult.Failure("Invalid Question ID.");

        var existingQuestion = await _quizRepository.GetQuestionByIdAsync(questionId);
        if (existingQuestion == null || existingQuestion.Isdeleted == true)
            return ValidationResult.Failure("Question does not exist or has been deleted.");

        // Check if the QuestionId exists in the Useranswer table
        bool isQuestionUsedInAnswers = await _quizRepository.IsQuestionUsedInAnswersAsync(questionId);
        if (isQuestionUsedInAnswers)
            return ValidationResult.Failure("This question cannot be deleted because it has been answered in one or more quizzes.");

        // Check if the QuestionId exists in the Quizquestion table and the related quiz is public
        bool isQuestionInPublicQuiz = await _quizRepository.IsQuestionInPublicQuizAsync(questionId);
        if (isQuestionInPublicQuiz)
            return ValidationResult.Failure("This question cannot be deleted because it is part of a public quiz.");

        return ValidationResult.Success();
    }

    public async Task<bool> SoftDeleteQuestionAsync(int id)
    {
        var question = await _quizRepository.GetQuestionByIdAsync(id);
        if (question == null || question.Isdeleted == true)
            return false;

        question.Isdeleted = true;
        await _quizRepository.UpdateQuestionAsync(question);
        return true;
    }
}
