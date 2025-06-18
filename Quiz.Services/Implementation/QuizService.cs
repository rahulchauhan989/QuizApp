using System.Net.Http.Json;
using System.Text.Json;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using quiz.Domain.DataModels;
using quiz.Domain.Dto;
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

    public async Task<ValidationResult> ValidateQuizForExistingQuestions(AddQuestionToQuizDto dto)
    {
        var quiz = await _quizRepository.GetQuizByIdAsync(dto.QuizId);

        if (quiz == null || quiz.Isdeleted == true)
            return ValidationResult.Failure("Quiz does not exist or has been deleted.");

        if (quiz.Ispublic == true)
            return ValidationResult.Failure("Cannot Add questions to a published quiz.");

        var existingQuestions = await _quizRepository.GetQuestionsByIdsAsync(dto.ExistingQuestionIds!);
        if (!existingQuestions.Any())
            return ValidationResult.Failure("No valid questions found for the provided IDs.");

        int totalMarks = existingQuestions.Sum(q => q.Marks) ?? 0;
        int QuizQuestionMarkSum = await _quizRepository.GetQuizQuestionsMarksSumAsync(dto.QuizId);
        int QuizTotalMarks = quiz.Totalmarks;

        if (totalMarks + QuizQuestionMarkSum > QuizTotalMarks)
            return ValidationResult.Failure($"Total marks of the Existing Question ({totalMarks})+ QuizQuestionMarkSum ({QuizQuestionMarkSum})   must be less than Totak Quiz Marks ({QuizTotalMarks}).");

        foreach (var question in existingQuestions)
        {
            bool isQuestionInQuiz = await _quizRepository.IsQuestionInQuizAsync(dto.QuizId, question.Id);
            if (isQuestionInQuiz)
                return ValidationResult.Failure($"Question with ID {question.Id} is already associated with the quiz.");

            if (question.CategoryId != quiz.Categoryid)
                return ValidationResult.Failure($"Question ID {question.Id} does not belong to the same category as the quiz.");
        }
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

        int quizQuestionsMarksSum = await _quizRepository.GetQuizQuestionsMarksSumAsync(dto.QuizId);

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

        foreach (var question in existingQuestions)
        {
            if (question.CategoryId != quiz.Categoryid)
                return ValidationResult.Failure($"Question ID {question.Id} does not belong to the same category as the quiz.");
        }
        return ValidationResult.Success();
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

        if (dto.QuestionIds == null || !dto.QuestionIds.Any())
            return ValidationResult.Failure("At least one question must be selected for the quiz.");

        var selectedQuestions = await _quizRepository.GetQuestionsByIdsAsync(dto.QuestionIds);
        int totalQuestionMarks = selectedQuestions.Sum(q => q.Marks) ?? 0;
        if (totalQuestionMarks != dto.Totalmarks)
        {
            return ValidationResult.Failure("Total marks of the quiz must match the sum of the marks of the selected questions.");
        }
        return ValidationResult.Success();
    }

    public async Task<ValidationResult> RemoveQuestionFromQuizAsyncValidation(int quizId, int questionId)
    {
        if (quizId <= 0 || questionId <= 0)
            return ValidationResult.Failure("Invalid Quiz ID or Question ID.");

        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
        if (quiz == null || quiz.Isdeleted == true)
            return ValidationResult.Failure("Quiz does not exist or has been deleted.");

        if (quiz.Ispublic == true)
            return ValidationResult.Failure("Cannot remove questions from a public quiz.");

        bool isQuestionInQuiz = await _quizRepository.IsQuestionInQuizAsync(quizId, questionId);
        if (!isQuestionInQuiz)
            return ValidationResult.Failure("Question does not exist in the specified quiz.");
        return ValidationResult.Success();
    }

    public async Task<ValidationResult> PublishQuizAsync(int quizId)
    {
        if (quizId <= 0)
            return ValidationResult.Failure("Invalid quiz ID.");

        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
        if (quiz == null)
            return ValidationResult.Failure("Quiz not found.");

        int quizQuestionsMarksSum = await _quizRepository.GetQuizQuestionsMarksSumAsync(quizId);

        if (quiz.Totalmarks != quizQuestionsMarksSum)
            return ValidationResult.Failure($"Quiz total marks ({quiz.Totalmarks}) must match the sum of the marks of its question's Mark ({quizQuestionsMarksSum}).");

        quiz.Ispublic = true;
        quiz.Modifiedat = DateTime.UtcNow.ToLocalTime();

        await _quizRepository.UpdateQuizAsync(quiz);

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> UnpublishQuizAsync(int quizId)
    {
        if (quizId <= 0)
            return ValidationResult.Failure("Invalid quiz ID.");

        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
        if (quiz == null)
            return ValidationResult.Failure("Quiz not found.");

        if (quiz.Ispublic == false)
            return ValidationResult.Failure("Quiz is already unpublished.");

        quiz.Ispublic = false;
        quiz.Modifiedat = DateTime.UtcNow.ToLocalTime();

        bool hasUnsubmittedAttempts = await _quizRepository.HasUnsubmittedAttemptsAsync(quizId);
        if (hasUnsubmittedAttempts)
            return ValidationResult.Failure("Cannot unpublish a quiz that is currently being attempted by someone.");

        await _quizRepository.UpdateQuizAsync(quiz);

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> validateForEditQuiz(QuizEditDto dto)
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

        bool isValidCategory = await _quizRepository.IsCategoryExistsAsync(dto.Categoryid);
        if (!isValidCategory)
            return ValidationResult.Failure("Category does not exist.");

        if (dto.Questions != null && dto.Questions.Any())
        {
            int totalQuestionMarks = dto.Questions.Sum(q => q.Marks);
            if (totalQuestionMarks < dto.Totalmarks)
                return ValidationResult.Failure($"Total marks of the quiz: {dto.Totalmarks} must be less than the sum of the marks of its questions.{totalQuestionMarks}");

            foreach (var question in dto.Questions)
            {
                if (string.IsNullOrWhiteSpace(question.Text))
                    return ValidationResult.Failure("Question text cannot be empty.");

                if (question.Marks <= 0)
                    return ValidationResult.Failure("Question marks must be greater than zero.");

                if (string.IsNullOrWhiteSpace(question.Difficulty) || !new[] { "Easy", "Medium", "Hard" }.Contains(question.Difficulty))
                    return ValidationResult.Failure($"Invalid difficulty level: {question.Difficulty}");

                if (question.Options == null || question.Options.Count != 4)
                    return ValidationResult.Failure("Each question must have four options.");

                if (!question.Options.Any(o => o.IsCorrect))
                    return ValidationResult.Failure("At least one option must be marked as correct.");

                if (question.Options.Count(o => o.IsCorrect) > 1)
                    return ValidationResult.Failure("Only one option can be marked as correct.");
            }
        }

        var quiz = await _quizRepository.GetQuizByIdAsync(dto.Id);
        if (quiz == null || quiz.Isdeleted == true)
            return ValidationResult.Failure("Quiz does not exist or has been deleted.");
        if (quiz.Ispublic == true)
            return ValidationResult.Failure("Cannot edit a published quiz.");
        return ValidationResult.Success();
    }

    public async Task<ValidationResult> validateForDeleteQuiz(int quizId)
    {
        if (quizId <= 0)
            return ValidationResult.Failure("Invalid quiz ID.");

        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
        if (quiz == null || quiz.Isdeleted == true)
            return ValidationResult.Failure("Quiz does not exist or has been deleted.");

        if (quiz.Ispublic == true)
            return ValidationResult.Failure("Cannot delete a published quiz.");

        bool hasUnsubmittedAttempts = await _quizRepository.HasUnsubmittedAttemptsAsync(quizId);
        if (hasUnsubmittedAttempts)
            return ValidationResult.Failure("Cannot delete a quiz because someone is attempting it.");

        return ValidationResult.Success();
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
            Ispublic = false, 
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
            Categoryid = (int)q.CategoryId!,
            Options = q.Options.Select(o => new OptionDto
            {
                Id = o.Id,
                Text = o.Text,
                IsCorrect = o.Iscorrect
            }).ToList()
        }).ToList();
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
            Createdby = userId,
        };

        await _quizRepository.CreateQuizAsync(quiz);

        if (dto.Questions != null && dto.Questions.Any())
        {
            foreach (var questionDto in dto.Questions)
            {
                var question = new Question
                {
                    Text = questionDto.Text!,
                    Marks = questionDto.Marks,
                    Difficulty = questionDto.Difficulty,
                    CategoryId = dto.Categoryid, 
                    Options = questionDto.Options?.Select(o => new Option
                    {
                        Text = o.Text!,
                        Iscorrect = o.IsCorrect,
                    }).ToList() ?? new List<Option>()
                };

                await _quizRepository.AddQuestionToQuizAsync(quiz, question);
            }
        }

        var questions = await _quizRepository.GetQuestionsByQuizIdAsync(quiz.Id);

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



    public async Task<QuizDto> CreateQuizFromExistingQuestionsAsync(CreateQuizFromExistingQuestionsDto dto)
    {
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

        await _quizRepository.CreateQuizAsync(quiz);

        foreach (var question in selectedQuestions)
        {
            await _quizRepository.AddQuestionToQuiz(quiz, question);
        }

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
        // Fetch the quiz
        var quiz = await _quizRepository.GetQuizByIdAsync(id);
        if (quiz == null || quiz.Isdeleted == true)
            return false;

        bool hasUnsubmittedAttempts = await _quizRepository.HasUnsubmittedAttemptsAsync(id);
        if (hasUnsubmittedAttempts)
            return false; 

        await _quizRepository.SoftDeleteQuizAsync(id);
        return true;
    }



    public async Task<bool> RemoveQuestionFromQuizAsync(RemoveQuestionFromQuizDto dto)
    {
        return await _quizRepository.RemoveQuestionFromQuizAsync(dto.QuizId, dto.QuestionId);
    }

    public async Task<IEnumerable<ActiveQuiz>> GetActiveQuizzesAsync()
    {
        return await _attemptRepo.GetActiveQuizzesAsync();
    }

    public async Task SubmitQuizAutomaticallyAsync(int attemptId)
    {
        var attempt = await _attemptRepo.GetAttemptByIdAsync(attemptId);
        if (attempt == null || attempt.Issubmitted == true)
            throw new Exception("Attempt not found or already submitted.");

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


}
