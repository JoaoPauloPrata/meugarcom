using FluentValidation;
using MeuGarcom.Application.Common.Messages;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MeuGarcom.Api.Http;

/// <summary>Validation errors → 400 with field errors; anything else → 500 without internals.</summary>
public sealed class ExceptionHandler(IProblemDetailsService problemDetails, ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        ProblemDetails problem;
        if (exception is ValidationException validation)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = validation.Errors
                .GroupBy(e => ToCamelCase(e.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
            problem = new HttpValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = UserMessages.ValidationTitle,
                Detail = errors.Values.SelectMany(v => v).FirstOrDefault() ?? UserMessages.ValidationTitle,
            };
            problem.Extensions["code"] = "validation";
        }
        else
        {
            logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Detail = UserMessages.Unexpected,
            };
            problem.Extensions["code"] = "unexpected";
        }

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static string ToCamelCase(string path) =>
        string.Join('.', path.Split('.').Select(p => p.Length == 0 ? p : char.ToLowerInvariant(p[0]) + p[1..]));
}
