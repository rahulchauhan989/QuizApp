using Microsoft.EntityFrameworkCore;
using quiz.Domain.DataContext;
using quiz.Domain.DataModels;
using quiz.Repo.Interface;

namespace quiz.Repo.Implementation;

public class UserQuizAttemptRepository : IUserQuizAttemptRepository
{
    private readonly QuiZappDbContext _context;

    public UserQuizAttemptRepository(QuiZappDbContext context)
    {
        _context = context;
    }

    public async Task<Userquizattempt?> GetAttemptByUserAndQuizAsync(int userId, int quizId, int categoryId)
    {
        return await _context.Userquizattempts
            .FirstOrDefaultAsync(a => a.Userid == userId && a.Quizid == quizId && a.Categoryid == categoryId);
    }

    public async Task<int> CreateAttemptAsync(Userquizattempt attempt)
    {
        _context.Userquizattempts.Add(attempt);
        await _context.SaveChangesAsync();
        return attempt.Id;
    }

    public async Task UpdateAttemptAsync(Userquizattempt attempt)
    {
        _context.Userquizattempts.Update(attempt);
        await _context.SaveChangesAsync();
    }

}
