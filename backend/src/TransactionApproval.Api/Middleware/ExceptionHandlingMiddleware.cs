using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TransactionApproval.Application.Common.Exceptions;

namespace TransactionApproval.Api.Middleware;

/// <summary>
/// Translates known exceptions into RFC 7807 ProblemDetails responses and keeps
/// controllers free of try/catch noise. Unexpected errors return a safe 500.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        ProblemDetails problem = exception switch
        {
            ValidationException validation => BuildValidationProblem(validation),
            NotFoundException notFound => Build(HttpStatusCode.NotFound, "Not Found", notFound.Message),
            ConflictException conflict => Build(HttpStatusCode.Conflict, "Conflict", conflict.Message),
            AuthenticationException auth => Build(HttpStatusCode.Unauthorized, "Unauthorized", auth.Message),
            _ => BuildUnexpected(exception)
        };

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }

    private static ProblemDetails Build(HttpStatusCode status, string title, string detail) => new()
    {
        Status = (int)status,
        Title = title,
        Detail = detail
    };

    private static ValidationProblemDetails BuildValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        };
    }

    private ProblemDetails BuildUnexpected(Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception");

        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = _environment.IsDevelopment() ? exception.ToString() : null
        };
    }
}
