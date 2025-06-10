using quiz.Domain.DataContext;
using quiz.Domain.DataModels;
using quiz.Repo.Interface;

namespace quiz.Repo.Implementation;

public class UserAnswerRepository : IUserAnswerRepository
{
    private readonly QuiZappDbContext _context;

    public UserAnswerRepository(QuiZappDbContext context)
    {
        _context = context;
    }

    public async Task SaveAnswerAsync(Useranswer answer)
    {
        _context.Useranswers.Add(answer);
        await _context.SaveChangesAsync();
    }
}
