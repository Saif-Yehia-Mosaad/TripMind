using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TripMind.Infrastructure.Persistence;

namespace TripMind.Infrastructure.BackgroundJobs;

public sealed class UnverifiedUserCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UnverifiedUserCleanupJob> _logger;

    public UnverifiedUserCleanupJob(
        IServiceScopeFactory scopeFactory,
        ILogger<UnverifiedUserCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<TripMindDbContext>();

                var expired = await db.Users
                    .Where(u =>
                        !u.IsEmailVerified &&
                        u.CreatedAt < DateTime.UtcNow.AddHours(-24))
                    .ToListAsync(stoppingToken);

                if (expired.Count > 0)
                {
                    db.Users.RemoveRange(expired);
                    await db.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unverified-user cleanup job failed; will retry next cycle.");
            }
        }
    }
}
