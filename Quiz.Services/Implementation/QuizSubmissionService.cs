using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quiz.Services.Interface;
using quiz.Domain.Dto;

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
                using (var scope = _serviceProvider.CreateScope())
                {
                    var quizService = scope.ServiceProvider.GetRequiredService<IQuizService>();

                    var activeQuizzes = await quizService.GetActiveQuizzesAsync();

                    foreach (var quiz in activeQuizzes)
                    {
                        if (quiz.StartedAt != DateTime.MinValue && quiz.DurationMinutes.HasValue &&
                            (DateTime.UtcNow - quiz.StartedAt).TotalMinutes >= quiz.DurationMinutes.Value)
                        {
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

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}

