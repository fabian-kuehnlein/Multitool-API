using System.Security.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Multitool.Domain.Exceptions;

namespace Multitool.Api.Exceptions;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService,ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred.");

        var problemDetails = exception switch
        {
            ArgumentException or InvalidOperationException => new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Bad request",
                Status = StatusCodes.Status400BadRequest,
                Detail = exception.Message
            },

            InvalidCredentialException => new ProblemDetails
            {
                Type = "https://httpstatuses.com/401",
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = exception.Message
            },

            NotFoundException or KeyNotFoundException => new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "Resource not found",
                Status = StatusCodes.Status404NotFound,
                Detail = exception.Message
            },

            UserAlreadyExistsException => new ProblemDetails
            {
                Type = "https://httpstatuses.com/409",
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = exception.Message
            },

            _ => new ProblemDetails
            {
                Type = "https://httpstatuses.com/500",
                Title = "Internal server error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred."
            }
        };

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}