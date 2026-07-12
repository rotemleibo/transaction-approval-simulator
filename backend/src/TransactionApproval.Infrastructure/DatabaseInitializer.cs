using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TransactionApproval.Infrastructure.Persistence;

namespace TransactionApproval.Infrastructure;

/// <summary>
/// Applies pending EF Core migrations at startup so a fresh container or clone
/// comes up with a ready schema and seeded region catalog (single-command demo).
/// </summary>
public static class DatabaseInitializer
{
    public static async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseInitializer));

        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Database is up to date.");
    }
}
