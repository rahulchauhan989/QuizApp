using Microsoft.EntityFrameworkCore;
using quiz.Domain.DataContext;
using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;
using quiz.Repo.Interface;

namespace quiz.Repo.Implementation;

public class QuizRepository : IQuizRepository
{
    private readonly QuiZappDbContext _context;

    public QuizRepository(QuiZappDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz> CreateQuizAsync(Quiz quiz)
    {
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();
        return quiz;
    }

    public async Task<IEnumerable<Question>> GetRandomQuestionsAsync(int Categoryid, int count)
    {
        return await _context.Questions
            .Where(q => q.CategoryId == Categoryid)
            .Include(q => q.Options)
            //database generates a random GUID for each row in the query result and sorts the rows based on these GUIDs, For large datasets, this can be inefficient and slow.
            // .OrderBy(q => Guid.NewGuid()) // Randomize the order
            // generates a random value between 0 and 1 for each row in the query
            .OrderBy(q => EF.Functions.Random()) //  PostgreSQL's RANDOM() function, This approach is more efficient because the randomization happens directly in the database.
            .Take(count)
            .ToListAsync();
    }

    public async Task<Question> CreateQuestionAsync(Question question)
    {
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task<int> GetQuestionCountByCategoryAsync(int Categoryid)
    {
        return await _context.Questions
            .CountAsync(q => q.CategoryId == Categoryid);
    }

    public async Task<bool> IsCategoryExistsAsync(int Categoryid)
    {
        return await _context.Categories
            .AnyAsync(c => c.Id == Categoryid);
    }

    public async Task<bool> IsQuizTitleExistsAsync(string title, int quizId)
    {
        return await _context.Quizzes
            .AnyAsync(q => q.Title == title && q.Id != quizId);
    }

    public async Task<IEnumerable<QuizListDto>> GetFilteredQuizzesAsync(QuizFilterDto filter)
    {
        var query = _context.Quizzes
            .Where(q => !q.Isdeleted!.Value)
            .Include(q => q.Category)
            .AsQueryable();

        if (filter.CategoryId.HasValue)
            query = query.Where(q => q.Categoryid == filter.CategoryId.Value);

        if (!string.IsNullOrEmpty(filter.TitleKeyword))
            query = query.Where(q => q.Title.ToLower().Trim().Contains(filter.TitleKeyword.ToLower().Trim()));

        if (filter.IsPublic.HasValue)
            query = query.Where(q => q.Ispublic == filter.IsPublic);

        return await query.Select(q => new QuizListDto
        {
            Id = q.Id,
            Title = q.Title,
            Description = q.Description,
            TotalMarks = q.Totalmarks,
            DurationMinutes = q.Durationminutes,
            IsPublic = q.Ispublic,
            CategoryName = q.Category.Name
        }).ToListAsync();
    }

    // public async Task<Userquizattempt> SubmitQuizAttemptAsync(SubmitQuizAttemptDto dto)
    // {
    //     //  Check if attempt already exists
    //     var existingAttempt = await _context.Userquizattempts
    //         .FirstOrDefaultAsync(x => x.Userid == dto.UserId && x.Quizid == dto.QuizId);

    //     if (existingAttempt != null)
    //     {
    //         throw new InvalidOperationException("User has already submitted this quiz.");
    //         // OR return existingAttempt; // if you want to return the previous attempt instead
    //     }

    //     var attempt = new Userquizattempt
    //     {
    //         Userid = dto.UserId,
    //         Quizid = dto.QuizId,
    //         Startedat = DateTime.UtcNow.ToLocalTime(),
    //         Endedat = DateTime.UtcNow.ToLocalTime(),
    //         Issubmitted = true
    //     };

    //     _context.Userquizattempts.Add(attempt);
    //     await _context.SaveChangesAsync();

    //     int totalScore = 0;

    //     foreach (var answer in dto.Answers!)
    //     {
    //         var option = await _context.Options.FindAsync(answer.OptionId);
    //         bool isCorrect = option?.Iscorrect ?? false;

    //         if (isCorrect)
    //         {
    //             var question = await _context.Questions.FindAsync(answer.QuestionId);
    //             totalScore += question?.Marks ?? 1;
    //         }

    //         var userAnswer = new Useranswer
    //         {
    //             Attemptid = attempt.Id,
    //             Questionid = answer.QuestionId,
    //             Optionid = answer.OptionId,
    //             Iscorrect = isCorrect
    //         };
    //         _context.Useranswers.Add(userAnswer);
    //     }

    //     attempt.Score = totalScore;
    //     attempt.Endedat = DateTime.UtcNow.ToLocalTime();

    //     await _context.SaveChangesAsync();

    //     return attempt;
    // }

    public async Task<List<CorrectAnswerDto>> GetCorrectAnswersForQuizAsync(int categoryid)
    {
        return await (from question in _context.Questions
                      join option in _context.Options
                      on question.Id equals option.Questionid
                      where question.CategoryId == categoryid && option.Iscorrect
                      select new CorrectAnswerDto
                      {
                          QuestionId = question.Id,
                          CorrectOptionId = option.Id
                      }).ToListAsync();
    }

    public async Task<int> GetTotalMarksByQuizIdAsync(SubmitQuizRequest request)
    {
        return await _context.Quizzes
            .Where(q => q.Id == request.QuizId)
            .Select(q => q.Totalmarks).FirstOrDefaultAsync();
    }

    public async Task<int> GetQuetionsMarkByIdAsync(int questionId)
    {
        int marks = await _context.Questions
            .Where(q => q.Id == questionId)
            .Select(q => q.Marks).FirstOrDefaultAsync() ?? 0;

            return marks;
    }

    public async Task<Quiz> GetQuizByIdAsync(int quizId)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Category)
            .FirstOrDefaultAsync(q => q.Id == quizId && !q.Isdeleted!.Value);

        if (quiz == null)
        {
            throw new InvalidOperationException($"Quiz with ID {quizId} not found.");
        }

        return quiz;
    }


}
