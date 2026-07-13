using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionApproval.Application.Approval;
using TransactionApproval.Application.Services;

namespace TransactionApproval.Application;

/// <summary>
/// Registers application-layer services: use-case services, the approval rule,
/// and FluentValidation validators.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<BankingHoursOptions>()
            .Bind(configuration.GetSection(BankingHoursOptions.SectionName))
            .Validate(
                options => options.Open < options.Close,
                "BankingHours configuration is invalid: Open must be earlier than Close.")
            .ValidateOnStart();

        services.AddScoped<ITransactionApprovalEvaluator, TransactionApprovalEvaluator>();
        services.AddScoped<IRegionService, RegionService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
