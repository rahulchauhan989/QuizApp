using Microsoft.Extensions.Logging;
using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;
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

    public async Task<IEnumerable<QuizListDto>> GetFilteredQuizzesAsync(QuizFilterDto filter)
    {
        return await _quizRepository.GetFilteredQuizzesAsync(filter);
    }


    #endregion


}
