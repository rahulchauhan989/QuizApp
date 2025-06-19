using Microsoft.Extensions.Logging;
using quiz.Domain.DataModels;
using quiz.Domain.Dto;
using quiz.Repo.Interface;
using Quiz.Services.Interface;

namespace Quiz.Services.Implementation;

public class QuizesSubmission : IQuizesSubmission
{
    private readonly IQuizRepository _quizRepository;
    private readonly ILogger<QuizesSubmission> _logger;

    private readonly IUserQuizAttemptRepository _attemptRepo;

    private readonly IUserAnswerRepository _answerRepo;
    public QuizesSubmission(IQuizRepository quizRepository, ILogger<QuizesSubmission> logger, IUserQuizAttemptRepository attemptRepo, IUserAnswerRepository answerRepo)
    {
        _attemptRepo = attemptRepo;
        _answerRepo = answerRepo;
        _quizRepository = quizRepository;
        _logger = logger;
    }

    public async Task<CreateQuizzDto> GetQuizByIdAsync(int quizId)
    {
        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
        if (quiz == null)
        {
            _logger.LogWarning($"Quiz with ID {quizId} not found.");
            return null!;
        }

        return new CreateQuizzDto
        {
            Id = quiz.Id,
            Title = quiz.Title!,
            Description = quiz.Description,
            Totalmarks = quiz.Totalmarks,
            Durationminutes = quiz.Durationminutes,
            Ispublic = quiz.Ispublic,
            Isdeleted = quiz.Isdeleted,
            Categoryid = quiz.Categoryid,
        };
    }
    public async Task<bool> CheckExistingAttemptAsync(int userId, int quizId, int categoryId)
    {
        var existingAttempt = await _attemptRepo.GetAttemptByUserAndQuizAsync(userId, quizId, categoryId);

        return existingAttempt != null;
    }

    public async Task<IEnumerable<QuestionDto>> GetQuestionsForQuizAsync(int quizId)
    {
        var questions = await _quizRepository.GetQuestionsByQuizIdAsync(quizId);

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
        return await _quizRepository.GetTotalMarksByQuizIdAsync(request);
    }

    public async Task<int> GetQuetionsMarkByIdAsync(int questionId)
    {
        return await _quizRepository.GetQuetionsMarkByIdAsync(questionId);
    }

    public async Task<int> SubmitQuizAsync(SubmitQuizRequest request)
    {
        var existingAttempt = await _attemptRepo.GetAttemptByUserAndQuizAsync(request.UserId, request.QuizId, request.categoryId);
        if (existingAttempt != null && existingAttempt.Issubmitted == true)
            return -1000;

        var correctAnswers = await _quizRepository.GetCorrectAnswersForQuizAsync(request.categoryId);

        int score = 0;
        foreach (var answer in request.Answers)
        {
            var correctOptionId = correctAnswers
                .FirstOrDefault(c => c.QuestionId == answer.QuestionId)?.CorrectOptionId;

            if (correctOptionId == answer.OptionId)
            {
                score += correctAnswers
                     .FirstOrDefault(c => c.QuestionId == answer.QuestionId)?.Marks ?? 0;
            }
            else
            {
                score -= correctAnswers
                    .FirstOrDefault(c => c.QuestionId == answer.QuestionId)?.Marks ?? 0;
            }
        }

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

            bool isCategoryExists = _quizRepository.IsCategoryExistsAsync(filter.CategoryId ?? 0).Result;
            if (filter.CategoryId.HasValue && !isCategoryExists)
                return ValidationResult.Failure($"Category with ID {filter.CategoryId} does not exist.");    

                

            return ValidationResult.Success();
        });
    }

    public async Task<ValidationResult> ValidateQuizSubmissionAsync(SubmitQuizRequest request)
    {
        return await Task.Run(async () =>
        {
            if (request == null)
                return ValidationResult.Failure("Request data is required.");

            if (request.UserId <= 0 || request.QuizId <= 0 || request.categoryId <= 0)
                return ValidationResult.Failure("Invalid User ID, Quiz ID, or Category ID.");

            var quiz = await _quizRepository.GetQuizByIdAsync(request.QuizId);
            if (quiz == null)
                return ValidationResult.Failure($"Quiz with ID {request.QuizId} does not exist.");

            if (request.Answers == null || !request.Answers.Any())
                return ValidationResult.Failure("At least one answer is required.");

            foreach (var answer in request.Answers)
            {
                if (answer.QuestionId <= 0 || answer.OptionId <= 0)
                    return ValidationResult.Failure("Invalid answer data.");
            }

            int totalMarks = await _quizRepository.GetTotalMarksByQuizIdAsync(request);
            int inputMarks = 0;

            foreach (var answer in request.Answers)
            {
                int questionMarks = await _quizRepository.GetQuetionsMarkByIdAsync(answer.QuestionId);
                inputMarks += questionMarks;
            }

            if (inputMarks > totalMarks)
                return ValidationResult.Failure($"Total marks for answers ({inputMarks}) exceed the quiz total marks ({totalMarks}).");

            if (quiz.Durationminutes.HasValue &&
                (request.EndedAt - request.StartedAt).TotalMinutes > quiz.Durationminutes.Value)
            {
                return ValidationResult.Failure($"Quiz duration exceeded. Maximum allowed is {quiz.Durationminutes} minutes.");
            }

            return ValidationResult.Success();
        });
    }

    public async Task<ValidationResult> ValidateQuizStartAsync(StartQuizRequest request)
    {
        return await Task.Run(async () =>
        {
            if (request == null)
                return ValidationResult.Failure("Request data is required.");

            if (request.UserId <= 0 || request.QuizId <= 0 || request.categoryId <= 0)
                return ValidationResult.Failure("Invalid User ID, Quiz ID, or Category ID.");

            var quiz = await _quizRepository.GetQuizByIdAsync(request.QuizId);
            if (quiz == null || quiz.Isdeleted == true)
                return ValidationResult.Failure($"Quiz with ID {request.QuizId} does not exist or is deleted.");

            if (quiz.Ispublic != true)
                return ValidationResult.Failure("This quiz is not public or active.");

            bool existingAttempt = await CheckExistingAttemptAsync(request.UserId, request.QuizId, request.categoryId);
            if (existingAttempt)
                return ValidationResult.Failure("You have already started this quiz or may have submitted it.");

            var questions = await GetQuestionsForQuizAsync(request.QuizId);
            if (questions == null || !questions.Any())
                return ValidationResult.Failure("No questions found for this quiz.");

            return ValidationResult.Success();
        });
    }

    public async Task<IEnumerable<QuizListDto>> GetFilteredQuizzesAsync(QuizFilterDto filter)
    {
        return await _quizRepository.GetFilteredQuizzesAsync(filter);
    }


}
