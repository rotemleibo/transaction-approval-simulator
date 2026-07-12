using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionApproval.Application.Abstractions;
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
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IRegionRepository, RegionRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
