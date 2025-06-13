using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quiz.Services.Interface;

public class QuizSubmissionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QuizSubmissionService> _logger;

    public QuizSubmissionService(IServiceProvider serviceProvider, ILogger<QuizSubmissionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Quiz Submission Service is running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a scope to resolve scoped services
                using (var scope = _serviceProvider.CreateScope())
                {
                    var quizService = scope.ServiceProvider.GetRequiredService<IQuizService>();

                    // Fetch active quizzes that need to be submitted
                    var activeQuizzes = await quizService.GetActiveQuizzesAsync();

                    foreach (var quiz in activeQuizzes)
                    {
                        // Check if the quiz duration has expired
                        if (quiz.StartedAt != DateTime.MinValue && quiz.DurationMinutes.HasValue &&
                            (DateTime.UtcNow - quiz.StartedAt).TotalMinutes >= quiz.DurationMinutes.Value)
                        {
                            // Automatically submit the quiz
                            await quizService.SubmitQuizAutomaticallyAsync(quiz.AttemptId);
                            _logger.LogInformation($"Quiz with Attempt ID {quiz.AttemptId} has been automatically submitted.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing quiz submissions.");
            }

            // Wait for a minute before checking again
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}




// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Quiz.Services.Interface;
// using quiz.Repo.Interface;

// public class QuizSubmissionService : BackgroundService
// {
//     private readonly IServiceProvider _serviceProvider;
//     private readonly ILogger<QuizSubmissionService> _logger;

//     public QuizSubmissionService(IServiceProvider serviceProvider, ILogger<QuizSubmissionService> logger)
//     {
//         _serviceProvider = serviceProvider;
//         _logger = logger;
//     }

//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         _logger.LogInformation("Quiz Submission Service is running.");

//         while (!stoppingToken.IsCancellationRequested)
//         {
//             try
//             {
//                 using (var scope = _serviceProvider.CreateScope())
//                 {
//                     var quizService = scope.ServiceProvider.GetRequiredService<IQuizService>();
//                     var attemptRepo = scope.ServiceProvider.GetRequiredService<IUserQuizAttemptRepository>();

//                     // Fetch active quiz attempts
//                     var activeAttempts = await attemptRepo.GetActiveAttemptsAsync();

//                     foreach (var attempt in activeAttempts)
//                     {
//                         // Check if the quiz duration has expired
//                         if (attempt.Startedat.HasValue && attempt.Categoryid.HasValue &&
//                             (DateTime.UtcNow - attempt.Startedat.Value).TotalMinutes >= attempt.Quiz.Durationminutes)
//                         {
//                             _logger.LogInformation($"Automatically submitting quiz attempt ID {attempt.Id}.");
//                             await quizService.SubmitQuizAutomaticallyAsync(attempt.Id);
//                         }
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error occurred while processing quiz submissions.");
//             }

//             // Wait for a minute before checking again
//             await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
//         }
//     }
// }