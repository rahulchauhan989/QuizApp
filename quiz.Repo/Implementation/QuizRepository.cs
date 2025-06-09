using Microsoft.EntityFrameworkCore;
using quiz.Domain.DataContext;
using quiz.Domain.DataModels;
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
            .OrderBy(q => Guid.NewGuid()) // Randomize the order
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
}
