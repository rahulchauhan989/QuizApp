using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;
using quiz.Repo.Interface;
using Quiz.Services.Interface;

namespace Quiz.Services.Implementation;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepository;

    private readonly IUserQuizAttemptRepository _attemptRepo;

    private readonly IUserAnswerRepository _answerRepo;

    public QuizService(IQuizRepository quizRepository, IUserQuizAttemptRepository attemptRepo, IUserAnswerRepository answerRepo)
    {
        _quizRepository = quizRepository;
        _attemptRepo = attemptRepo;
        _answerRepo = answerRepo;
    }
 


    public async Task<QuizDto> CreateQuizAsync(CreateQuizDto dto)
    {
        var quiz = new quiz.Domain.DataModels.Quiz
        {
            Title = dto.Title!,
            Description = dto.Description,
            Totalmarks = dto.Totalmarks,
            Durationminutes = dto.Durationminutes,
            Ispublic = dto.Ispublic,
            Startdate = dto.Startdate,
            Enddate = dto.Enddate,
            Categoryid = dto.Categoryid,
            Createdby = dto.Createdby,
        };

        if (dto.Questions != null)
        {
            quiz.Questions = dto.Questions.Select(q => new Question
            {
                Text = q.Text!,
                Marks = q.Marks,
                Difficulty = q.Difficulty,
                CategoryId = dto.Categoryid, // Assuming all questions belong to the same category
                Options = q.Options?.Select(o => new Option
                {
                    Text = o.Text!,
                    Iscorrect = o.IsCorrect
                }).ToList() ?? new List<Option>()
            }).ToList();
        }

        if (dto.TagIds != null)
        {
            quiz.Quiztags = dto.TagIds.Select(tagId => new Quiztag { Tagid = tagId }).ToList();
        }

        await _quizRepository.CreateQuizAsync(quiz);

        // Return the output DTO 
        return new QuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title!,
            Description = quiz.Description,
            Totalmarks = quiz.Totalmarks,
            Durationminutes = quiz.Durationminutes,
            Ispublic = quiz.Ispublic,
            Startdate = quiz.Startdate,
            Enddate = quiz.Enddate,
            Categoryid = quiz.Categoryid,
            Createdby = quiz.Createdby,
            Questions = quiz.Questions?.Select(q => new QuestionDto
            {
                Id = q.Id,
                QuizId = q.Quizid,
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
            }).ToList()
        };
    }

    public async Task<List<QuestionDto>> GetRandomQuestionsAsync(int Categoryid, int count)
    {
        var questions = await _quizRepository.GetRandomQuestionsAsync(Categoryid, count);

        return questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            QuizId = q.Quizid,
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

    public async Task<QuestionDto> CreateQuestionAsync(QuestionCreateDto dto)
    {
        var question = new Question
        {
            Quizid = dto.QuizId,
            CategoryId = dto.Categoryid,
            Text = dto.Text!,  //
            Marks = dto.Marks,
            Difficulty = dto.Difficulty,
            Options = dto.Options!.Select(o => new Option
            {
                Text = o.Text!,
                Iscorrect = o.IsCorrect,
            }).ToList()
        };

        await _quizRepository.CreateQuestionAsync(question);

        // Map to DTO
        return new QuestionDto
        {
            Id = question.Id,
            QuizId = question.Quizid,
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

    public async Task<int> GetQuestionCountByCategoryAsync(int Categoryid)
    {
        return await _quizRepository.GetQuestionCountByCategoryAsync(Categoryid);
    }

    public async Task<bool> IsCategoryExistsAsync(int Categoryid)
    {
        bool result = await _quizRepository.IsCategoryExistsAsync(Categoryid);

        return result;
    }

    public async Task<bool> IsQuizTitleExistsAsync(string title, int quizId)
    {
        return await _quizRepository.IsQuizTitleExistsAsync(title, quizId);
    }

    public async Task<ValidationResult> ValidateQuizAsync(CreateQuizDto dto)
    {
        return await Task.Run(() =>
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

            if (dto.Startdate >= dto.Enddate)
                return ValidationResult.Failure("Start date must be before end date.");

            if (dto.Enddate - dto.Startdate != TimeSpan.FromMinutes(dto.Durationminutes!.Value))
                return ValidationResult.Failure("Duration does not match the difference between start and end date.");

            if (dto.Categoryid <= 0)
                return ValidationResult.Failure("Invalid Category ID.");

            if (dto.Createdby <= 0)
                return ValidationResult.Failure("Invalid Creator ID.");

            // Validate questions if provided
            if (dto.Questions != null && dto.Questions.Any())
            {
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

    public async Task<ValidationResult> ValidateQuestionAsync(QuestionCreateDto dto)
    {

        return await Task.Run(() =>
         {
             string[] difficultyLevels = { "Easy", "Medium", "Hard", "easy", "medium", "hard" };

             if (dto == null)
                 return ValidationResult.Failure("Question data is required.");

             if (dto.QuizId <= 0)
                 return ValidationResult.Failure("Invalid Quiz ID.");

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

    public async Task<IEnumerable<QuizListDto>> GetFilteredQuizzesAsync(QuizFilterDto filter)
    {
        return await _quizRepository.GetFilteredQuizzesAsync(filter);
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

    // public Task<Userquizattempt> SubmitQuizAttemptAsync(SubmitQuizAttemptDto dto)
    // {
    //     return _quizRepository.SubmitQuizAttemptAsync(dto);
    // }

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
                Startdate = quiz.Startdate,
                Enddate = quiz.Enddate,
                Categoryid = quiz.Categoryid,
                
            };
     }
 
}

