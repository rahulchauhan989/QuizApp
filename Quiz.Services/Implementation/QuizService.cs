using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;
using quiz.Repo.Interface;
using Quiz.Services.Interface;

namespace Quiz.Services.Implementation;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepository;

    public QuizService(IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }

    // public async Task<QuizDto> CreateQuizAsync(quiz.Domain.DataModels.Quiz quiz)
    // {
    //     await _quizRepository.CreateQuizAsync(quiz);

    //     return new QuizDto
    //     {
    //         Id = quiz.Id,
    //         Title = quiz.Title,
    //         Description = quiz.Description,
    //         Totalmarks = quiz.Totalmarks,
    //         Durationminutes = quiz.Durationminutes,
    //         Ispublic = quiz.Ispublic,
    //         Startdate = quiz.Startdate,
    //         Enddate = quiz.Enddate,
    //         Categoryid = quiz.Categoryid,
    //         Createdby = quiz.Createdby
    //         // Not including Questions here to keep it lightweight
    //     };
    // }


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


    // public Task<IEnumerable<Question>> GetRandomQuestionsAsync(int quizId, int count)
    // {
    //     return _quizRepository.GetRandomQuestionsAsync(quizId, count);
    // }

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

    // public async Task<Question> CreateQuestionAsync(QuestionCreateDto dto)
    // {
    //     var question = new Question
    //     {
    //         Quizid = dto.QuizId,
    //         Text = dto.Text,
    //         Marks = dto.Marks,
    //         Difficulty = dto.Difficulty,
    //         Options = dto.Options.Select(o => new Option
    //         {
    //             Text = o.Text,
    //             Iscorrect = o.IsCorrect,
    //         }).ToList()
    //     };

    //     return await _quizRepository.CreateQuestionAsync(question);
    // }
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

    public async Task<bool> IsQuizTitleExistsAsync(string title,int quizId)
    {
        // Assuming you have a method in your repository to check if a quiz title exists
        return await _quizRepository.IsQuizTitleExistsAsync(title,quizId);
    }

}

