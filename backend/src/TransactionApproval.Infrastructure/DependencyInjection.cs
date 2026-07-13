using EntityFramework.Exceptions.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionApproval.Application.Abstractions;
using TransactionApproval.Domain.Entities;
using TransactionApproval.Infrastructure.Messaging;
using TransactionApproval.Infrastructure.Persistence;
using TransactionApproval.Infrastructure.Persistence.Repositories;
using TransactionApproval.Infrastructure.Security;
using TransactionApproval.Infrastructure.Time;

namespace TransactionApproval.Infrastructure;

/// <summary>
/// Registers infrastructure concerns: EF Core, repositories, clock, and the
/// security services (password hashing + JWT issuance).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options
                .UseSqlServer(
                    configuration.GetConnectionString("Default"),
                    sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                .UseExceptionProcessor());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<OutboxDispatcherOptions>(configuration.GetSection(OutboxDispatcherOptions.SectionName));

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IRegionRepository, RegionRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IOutboxEventSerializer, OutboxEventSerializer>();
        services.AddScoped<IEventPublisher, LogEventPublisher>();
        services.AddHostedService<OutboxDispatcherService>();

        return services;
    }
}
