using Microsoft.AspNetCore.Mvc;
using Multitool.Api.Exceptions;

namespace Multitool.Api.Extensions;

public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(configure =>
        {
            configure.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            };
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Handles errors that occur during model binding/validation ([ApiController]),
        // before any controller or the GlobalExceptionHandler is reached
        // (e.g. invalid enum values in the request body), in the same ProblemDetails format.
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Type = "https://httpstatuses.com/400",
                    Title = "Bad request",
                    Status = StatusCodes.Status400BadRequest
                };
                problemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });

        return services;
    }
}
