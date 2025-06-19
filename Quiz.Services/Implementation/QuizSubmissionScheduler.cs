using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quiz.Services.Interface;

namespace Quiz.Services.Implementation
{
    public class QuizSubmissionScheduler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QuizSubmissionScheduler> _logger;

        public QuizSubmissionScheduler(IServiceProvider serviceProvider, ILogger<QuizSubmissionScheduler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void ScheduleQuizSubmission(int attemptId, DateTime startedAt, int durationMinutes)
        {
            var delay = TimeSpan.FromMinutes(durationMinutes) - (DateTime.UtcNow - startedAt);

            if (delay > TimeSpan.Zero)
            {
                Task.Delay(delay).ContinueWith(async _ =>
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var quizService = scope.ServiceProvider.GetRequiredService<IQuizService>();
                            await quizService.SubmitQuizAutomaticallyAsync(attemptId);
                            _logger.LogInformation($"Quiz with Attempt ID {attemptId} has been automatically submitted.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error occurred while submitting quiz with Attempt ID {attemptId}.");
                    }
                });
            }
            else
            {
                _logger.LogWarning($"Quiz with Attempt ID {attemptId} has already expired.");
            }
        }
    }
}