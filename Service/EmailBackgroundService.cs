using SOLFranceBackend.Interfaces;
using SOLFranceBackend.Interfaces.Implementation;

namespace SOLFranceBackend.Service
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly EmailQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(
            EmailQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<EmailBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var email in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                await SendWithRetryAsync(email, stoppingToken);
            }
        }

        private async Task SendWithRetryAsync(
            EmailMessage email,
            CancellationToken cancellationToken)
        {
            const int maxRetries = 3;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var emailSender = scope.ServiceProvider
                        .GetRequiredService<IEmailSender>();

                    await emailSender.SendEmailAsync(
                        email.To,
                        email.Subject,
                        email.Body);

                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Email send failed (Attempt {Attempt}/{Max}) for {Email}",
                        attempt, maxRetries, email.To);

                    if (attempt == maxRetries)
                    {
                        _logger.LogError(
                            "Email permanently failed for {Email}", email.To);
                    }

                    await Task.Delay(
                        TimeSpan.FromSeconds(2 * attempt),
                        cancellationToken);
                }
            }
        }
    }


}
