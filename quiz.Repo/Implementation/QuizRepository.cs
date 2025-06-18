using Microsoft.EntityFrameworkCore;
using quiz.Domain.DataContext;
using quiz.Domain.DataModels;
using quiz.Domain.Dto;

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

    public async Task LinkExistingQuestionToQuizAsync(int quizId, int questionId)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "INSERT INTO QuizQuestions (QuizId, QuestionId) VALUES ({0}, {1})",
            quizId, questionId
        );
    }

    public async Task AddQuestionToQuizAsync(Quiz quiz, Question question)
    {
        // Add the question to the database if it doesn't already exist
        if (question.Id == 0)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
        }

        // Use raw SQL to insert into the QuizQuestions table
        await _context.Database.ExecuteSqlRawAsync(
            "INSERT INTO QuizQuestions (QuizId, QuestionId) VALUES ({0}, {1})",
            quiz.Id, question.Id
        );
    }

    public async Task<IEnumerable<Question>> GetRandomQuestionsAsync(int Categoryid, int count)
    {
        return await _context.Questions
            .Where(q => q.CategoryId == Categoryid && q.Isdeleted == false) 
            .Include(q => q.Options)
            .OrderBy(q => EF.Functions.Random()) 
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetRandomQuestionsByQuizIdAsync(int quizId, int count)
    {
        return await _context.Questions
            .Where(q => _context.Quizquestions
                .Where(qq => qq.Quizid == quizId)
                .Select(qq => qq.Questionid)
                .Contains(q.Id)) 
            .Include(q => q.Options) 
            .OrderBy(q => EF.Functions.Random()) 
            .Take(count)
            .ToListAsync();
    }

    public async Task UpdateQuizAsync(Quiz quiz)
    {
        _context.Quizzes.Update(quiz);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteQuizAsync(int id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz != null)
        {
            quiz.Isdeleted = true;
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetQuestionCountByQuizIdAsync(int quizId)
    {
        return await _context.Quizquestions
            .Where(qq => qq.Quizid == quizId)
            .CountAsync();
    }

    public async Task<bool> IsQuizExistsAsync(int quizId)
    {
        return await _context.Quizzes.AnyAsync(q => q.Id == quizId);
    }

    public async Task<Question> CreateQuestionAsync(Question question)
    {
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return question;
    }
    public async Task<IEnumerable<Question>> GetQuestionsByQuizIdAsync(int quizId)
    {
        return await _context.Questions
            .Where(q => _context.Quizquestions
                .Where(qq => qq.Quizid == quizId)
                .Select(qq => qq.Questionid)
                .Contains(q.Id))
            .Include(q => q.Options) 
            .ToListAsync();
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

    public async Task<bool> IsQuizTitleExistsAsync(string title)
    {
        return await _context.Quizzes
            .AnyAsync(q => q.Title == title);
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

    public async Task<List<CorrectAnswerDto>> GetCorrectAnswersForQuizAsync(int categoryid)
    {
        return await (from question in _context.Questions
                      join option in _context.Options
                      on question.Id equals option.Questionid
                      where question.CategoryId == categoryid && option.Iscorrect
                      select new CorrectAnswerDto
                      {
                          QuestionId = question.Id,
                          CorrectOptionId = option.Id,
                            Marks =(int) question.Marks!
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
            .FirstOrDefaultAsync(q => q.Id == quizId && q.Isdeleted == false);

        if (quiz == null)
        {
            throw new InvalidOperationException($"Quiz with ID {quizId} not found.");
        }

        return quiz;
    }

    public async Task<IEnumerable<Question>> GetQuestionsByIdsAsync(List<int> questionIds)
    {
        return await _context.Questions
            .Where(q => questionIds.Contains(q.Id))
            .Include(q => q.Options) 
            .ToListAsync();
    }

    public async Task AddQuestionToQuiz(Quiz quiz, Question question)
    {
        var quizQuestion = new Quizquestion
        {
            Quizid = quiz.Id,
            Questionid = question.Id
        };

        await _context.Database.ExecuteSqlRawAsync(
          "INSERT INTO QuizQuestions (QuizId, QuestionId) VALUES ({0}, {1})",
          quizQuestion.Quizid, quizQuestion.Questionid
      );
        await _context.SaveChangesAsync();
    }

    public async Task<List<Userquizattempt>> GetUserQuizAttemptsAsync(int userId)
    {
        return await _context.Userquizattempts
            .Where(attempt => attempt.Userid == userId)
            .Include(attempt => attempt.Quiz) 
            .Include(attempt => attempt.Category) 
            .Include(attempt => attempt.Useranswers) 
                .ThenInclude(answer => answer.Question) 
            .Include(attempt => attempt.Useranswers)
                .ThenInclude(answer => answer.Option) 
            .ToListAsync();
    }

    public async Task<int> GetQuizQuestionsMarksSumAsync(int quizId)
    {
        return (int)await _context.Quizquestions
            .Where(qq => qq.Quizid == quizId)
            .Join(_context.Questions, qq => qq.Questionid, q => q.Id, (qq, q) => q.Marks)
            .SumAsync();
    }

    public async Task<bool> RemoveQuestionFromQuizAsync(int quizId, int questionId)
    {
        var affected = await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM QuizQuestions WHERE QuizId = {0} AND QuestionId = {1}",
            quizId, questionId
        );

        return affected > 0;
    }

    public async Task<Question?> GetQuestionByIdAsync(int questionId)
    {
        return await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId);
    }

    public async Task UpdateQuestionAsync(Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveOptionsByQuestionIdAsync(int questionId)
    {
        var options = _context.Options.Where(o => o.Questionid == questionId);
        _context.Options.RemoveRange(options);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsQuestionUsedInAnswersAsync(int questionId)
    {
        return await _context.Useranswers.AnyAsync(ua => ua.Questionid == questionId);
    }

    public async Task<bool> IsQuestionInPublicQuizAsync(int questionId)
    {
        return await _context.Quizquestions
            .Where(q => q.Questionid == questionId)
            .AnyAsync(q => q.Quiz.Ispublic == true);
    }

    public async Task<bool> IsQuestionInQuizAsync(int quizId, int questionId)
    {
        return await _context.Quizquestions.AnyAsync(q => q.Quizid == quizId && q.Questionid == questionId);
    }

    public async Task<bool> HasUnsubmittedAttemptsAsync(int quizId)
    {
        return await _context.Userquizattempts.AnyAsync(uqa => uqa.Quizid == quizId && uqa.Issubmitted == false);
    }

}
