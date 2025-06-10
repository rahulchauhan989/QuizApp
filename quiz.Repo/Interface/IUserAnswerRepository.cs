using quiz.Domain.DataModels;

namespace quiz.Repo.Interface;

public interface IUserAnswerRepository
{
     Task SaveAnswerAsync(Useranswer answer);
}
