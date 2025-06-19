using Microsoft.EntityFrameworkCore;
using quiz.Domain.DataContext;
using quiz.Domain.DataModels;
using quiz.Domain.Dto;
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
            .FirstOrDefaultAsync(a => a.Userid == userId && a.Quizid == quizId);
    }

    public async Task<int> CreateAttemptAsync(Userquizattempt attempt)
    {
        if (attempt.Startedat.HasValue)
            attempt.Startedat = DateTime.SpecifyKind(attempt.Startedat.Value, DateTimeKind.Unspecified);

        if (attempt.Endedat.HasValue)
            attempt.Endedat = DateTime.SpecifyKind(attempt.Endedat.Value, DateTimeKind.Unspecified);

        _context.Userquizattempts.Add(attempt);
        await _context.SaveChangesAsync();
        return attempt.Id;
    }

    public async Task UpdateAttemptAsync(Userquizattempt attempt)
    {
        if (attempt.Startedat.HasValue)
            attempt.Startedat = DateTime.SpecifyKind(attempt.Startedat.Value, DateTimeKind.Unspecified);

        if (attempt.Endedat.HasValue)
            attempt.Endedat = DateTime.SpecifyKind(attempt.Endedat.Value, DateTimeKind.Unspecified);

        _context.Userquizattempts.Update(attempt);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ActiveQuiz>> GetActiveQuizzesAsync()
    {
        var attempts = await _context.Userquizattempts
            .Where(a => a.Issubmitted == false && a.Startedat.HasValue)
            .Include(a => a.Quiz)
            .ToListAsync();

        return attempts.Select(a => new ActiveQuiz
        {
            AttemptId = a.Id,
            StartedAt = a.Startedat!.Value,
            DurationMinutes = a.Quiz?.Durationminutes
        }).ToList();
    }

    public async Task<Userquizattempt?> GetAttemptByIdAsync(int attemptId)
    {
        return await _context.Userquizattempts
            .Include(a => a.Useranswers)
            .FirstOrDefaultAsync(a => a.Id == attemptId);
    }

    public async Task<Userquizattempt?> GetAttemptByUserAndQuizAsync(int userId, int quizId)
    {
        return await _context.Userquizattempts
            .FirstOrDefaultAsync(a => a.Userid == userId && a.Quizid == quizId);
    }

    public async Task<bool> IsUserExistAsync(int userId)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId);
    }

    public async Task<IEnumerable<Userquizattempt>> GetAttemptsByCategoryIdAsync(int categoryId)
    {
        return await _context.Userquizattempts
            .Where(uqa => uqa.Categoryid == categoryId)
            .ToListAsync();
    }
}
