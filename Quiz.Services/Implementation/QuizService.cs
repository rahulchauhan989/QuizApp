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

    // public QuizService(IQuizRepository quizRepository, IUserQuizAttemptRepository attemptRepo, IUserAnswerRepository answerRepo)
    // {
    //     _quizRepository = quizRepository;
    //     _attemptRepo = attemptRepo;
    //     _answerRepo = answerRepo;
    // }

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
            // Startdate = quiz.Startdate,
            // Enddate = quiz.Enddate,
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

        // if (dto.Startdate >= dto.Enddate)
        //     return ValidationResult.Failure("Start date must be before end date.");

        // if (dto.Enddate - dto.Startdate != TimeSpan.FromMinutes(dto.Durationminutes!.Value))
        //     return ValidationResult.Failure("Duration does not match the difference between start and end date.");

        if (dto.Categoryid <= 0)
            return ValidationResult.Failure("Invalid Category ID.");


        if (dto.QuestionIds == null || !dto.QuestionIds.Any())
            return ValidationResult.Failure("At least one question must be selected.");

        var selectedQuestions = await _quizRepository.GetQuestionsByIdsAsync(dto.QuestionIds);
        int totalQuestionMarks = selectedQuestions.Sum(q => q.Marks) ?? 0;
        if (totalQuestionMarks != dto.Totalmarks)
        {
            return ValidationResult.Failure("Total marks of the quiz must match the sum of the marks of the selected questions.");
        }

        return ValidationResult.Success();

    }


    public async Task<ValidationResult> ValidateQuizFilterAsync(QuizFilterDto filter)
    {
        return await Task.Run(() =>
        {
            if (filter == null)
                return ValidationResult.Failure("Filter data is required.");

            if (filter.CategoryId.HasValue && filter.CategoryId <= 0)
                return ValidationResult.Failure("Invalid Category ID.");

            if (!string.IsNullOrEmpty(filter.TitleKeyword) && filter.TitleKeyword.Length < 3)
                return ValidationResult.Failure("Title keyword must be at least 3 characters long.");

            return ValidationResult.Success();
        });
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
            // Startdate = quiz.Startdate,
            // Enddate = quiz.Enddate,
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


    public async Task<IEnumerable<QuizListDto>> GetFilteredQuizzesAsync(QuizFilterDto filter)
    {
        return await _quizRepository.GetFilteredQuizzesAsync(filter);
    }

    #endregion


    #region Question Management

    public async Task<ValidationResult> ValidateQuestionAsync(QuestionCreateDto dto)
    {
        return await Task.Run(() =>
         {
             string[] difficultyLevels = { "Easy", "Medium", "Hard", "easy", "medium", "hard" };

             if (dto == null)
                 return ValidationResult.Failure("Question data is required.");

             if (dto.Categoryid <= 0)
                 return ValidationResult.Failure("Invalid Category ID.");

             if (string.IsNullOrWhiteSpace(dto.Text))
                 return ValidationResult.Failure("Question text cannot be empty.");

             if (dto.Marks <= 0)
                 return ValidationResult.Failure("Marks must be greater than zero.");

             if (string.IsNullOrWhiteSpace(dto.Difficulty) || !difficultyLevels.Contains(dto.Difficulty))
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

    public async Task<QuestionDto> CreateQuestionAsync(QuestionCreateDto dto)
    {
        var question = new Question
        {
            CategoryId = dto.Categoryid,
            Text = dto.Text!,
            Marks = dto.Marks,
            Difficulty = dto.Difficulty,
            Options = dto.Options!.Select(o => new Option
            {
                Text = o.Text!,
                Iscorrect = o.IsCorrect,
            }).ToList()
        };

        // Add the question to the database
        await _quizRepository.CreateQuestionAsync(question);

        // Associate the question with the specified quiz, For Now No need to associate with quiz
        // await _quizRepository.AddQuestionToQuizAsync(dto.QuizId, question.Id);

        // Map to DTO
        return new QuestionDto
        {
            Id = question.Id,
            // QuizId = dto.QuizId,
            Categoryid = (int)question.CategoryId!,
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
            // QuizId = q.Quizid,
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
            QuizId = quizId,
            Categoryid = q.CategoryId ?? 0, // Default to 0 if null
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
        // return null;
    }

    public async Task<bool> IsQuizExistsAsync(int quizId)
    {
        return await _quizRepository.IsQuizExistsAsync(quizId);
    }


    #endregion

    #region Quiz Submission
    public async Task<CreateQuizViewModel> GetQuizByIdAsync(int quizId)
    {
        // Fetch the quiz details by ID
        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
        if (quiz == null)
            throw new Exception("Quiz not found.");

        // Map to CreateQuizViewModel
        return new CreateQuizViewModel
        {
            Id = quiz.Id,
            Title = quiz.Title!,
            Description = quiz.Description,
            Totalmarks = quiz.Totalmarks,
            Durationminutes = quiz.Durationminutes,
            Ispublic = quiz.Ispublic,
            Isdeleted = quiz.Isdeleted,
            // Startdate = quiz.Startdate,
            // Enddate = quiz.Enddate,
            Categoryid = quiz.Categoryid,
        };
    }
    public async Task<bool> CheckExistingAttemptAsync(int userId, int quizId, int categoryId)
    {
        // Fetch existing attempt
        var existingAttempt = await _attemptRepo.GetAttemptByUserAndQuizAsync(userId, quizId, categoryId);

        // Return true if an attempt exists and is not submitted
        return existingAttempt != null;
    }

    public async Task<IEnumerable<QuestionDto>> GetQuestionsForQuizAsync(int quizId)
    {
        // Fetch questions for the quiz from the database
        var questions = await _quizRepository.GetQuestionsByQuizIdAsync(quizId);

        // Map questions to DTOs
        return questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            Text = q.Text,
            Options = q.Options.Select(o => new OptionDto
            {
                Id = o.Id,
                Text = o.Text
            }).ToList()
        });
    }

    public async Task<int> StartQuizAsync(int userId, int quizId, int categoryId)
    {
        // Create a new quiz attempt
        var attemptId = await _attemptRepo.CreateAttemptAsync(new Userquizattempt
        {
            Userid = userId,
            Quizid = quizId,
            Startedat = DateTime.UtcNow,
            Issubmitted = false,
            Categoryid = categoryId
        });

        return attemptId;
    }

    public async Task<int> GetTotalMarksAsync(SubmitQuizRequest request)
    {
        // Get the total marks for the quiz based on the category
        return await _quizRepository.GetTotalMarksByQuizIdAsync(request);
    }

    public async Task<int> GetQuetionsMarkByIdAsync(int questionId)
    {
        // Get the marks for a specific question by its ID
        return await _quizRepository.GetQuetionsMarkByIdAsync(questionId);
    }

    public async Task<int> SubmitQuizAsync(SubmitQuizRequest request)
    {
        // 1. Validate if already submitted
        var existingAttempt = await _attemptRepo.GetAttemptByUserAndQuizAsync(request.UserId, request.QuizId, request.categoryId);
        if (existingAttempt != null && existingAttempt.Issubmitted == true)
            throw new Exception("Quiz already submitted.");

        // 2. Get correct answers from DB
        var correctAnswers = await _quizRepository.GetCorrectAnswersForQuizAsync(request.categoryId);

        // 3. Calculate score
        int score = 0;
        foreach (var answer in request.Answers)
        {
            var correctOptionId = correctAnswers
                .FirstOrDefault(c => c.QuestionId == answer.QuestionId)?.CorrectOptionId;

            if (correctOptionId == answer.OptionId)
                score++;
        }

        // 4. Create or update attempt
        //existingAttempt?.Id ?? is not null than left side value will be used, otherwise right side value will be used
        var attemptId = existingAttempt?.Id ?? await _attemptRepo.CreateAttemptAsync(new Userquizattempt
        {
            Userid = request.UserId,
            Quizid = request.QuizId,
            Categoryid = request.categoryId,
            Startedat = request.StartedAt.ToLocalTime(),
            Endedat = request.EndedAt.ToLocalTime(),
            Timespent = (int)(request.EndedAt - request.StartedAt).TotalMinutes,
            Score = score,
            Issubmitted = true
        });

        // 5. Save all user answers
        foreach (var ans in request.Answers)
        {
            var isCorrect = correctAnswers
                .FirstOrDefault(c => c.QuestionId == ans.QuestionId)?.CorrectOptionId == ans.OptionId;

            await _answerRepo.SaveAnswerAsync(new Useranswer
            {
                Attemptid = attemptId,
                Questionid = ans.QuestionId,
                Optionid = ans.OptionId,
                Iscorrect = isCorrect
            });
        }
        return score;
    }


    #endregion


    #region User Quiz History

    public async Task<List<UserQuizHistoryDto>> GetUserQuizHistoryAsync(int userId)
    {
        // Fetch user quiz attempts from the repository
        var userQuizAttempts = await _quizRepository.GetUserQuizAttemptsAsync(userId);

        // Map the data to the DTO
        return userQuizAttempts.Select(attempt => new UserQuizHistoryDto
        {
            AttemptId = attempt.Id,
            QuizId = attempt.Quizid,
            QuizTitle = attempt.Quiz.Title,
            Score = attempt.Score,
            TimeSpent = attempt.Timespent,
            StartedAt = attempt.Startedat,
            EndedAt = attempt.Endedat,
            IsSubmitted = attempt.Issubmitted,
            CategoryId = attempt.Categoryid,
            CategoryName = attempt.Category?.Name,
            UserAnswers = attempt.Useranswers.Select(answer => new UserAnswerDto
            {
                QuestionId = answer.Questionid,
                QuestionText = answer.Question.Text,
                OptionId = answer.Optionid,
                OptionText = answer.Option.Text,
                IsCorrect = answer.Iscorrect
            }).ToList()
        }).ToList();
    }

    #endregion

    // public async Task<quiz.Domain.DataModels.Quiz> GetQuizById(int quizId)
    // {
    //     // Fetch the quiz from the repository
    //     var quiz = await _quizRepository.GetQuizByIdAsync(quizId);

    //     if (quiz == null)
    //         throw new Exception($"Quiz with ID {quizId} does not exist.");

    //     return quiz;
    // }


    #region Automatic Quiz Submission
    public async Task<IEnumerable<ActiveQuiz>> GetActiveQuizzesAsync()
    {
        // Fetch active quizzes that are not yet submitted
        return await _attemptRepo.GetActiveQuizzesAsync();
    }

    public async Task SubmitQuizAutomaticallyAsync(int attemptId)
    {

        // string rawRequestBody;
        // using (var reader = new StreamReader(_httpContextAccessor.HttpContext.Request.Body))
        // {
        //     rawRequestBody = await reader.ReadToEndAsync();
        // }


        // // Deserialize the JSON payload into the SubmitQuizRequest
        // var model = JsonSerializer.Deserialize<SubmitQuizRequest>(rawRequestBody);
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
