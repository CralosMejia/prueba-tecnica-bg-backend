using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;

namespace ShoppingCart.Api.Errors;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}",
            httpContext.TraceIdentifier
        );

        var error = MapException(exception);

        if (error.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Request failed with status {StatusCode}. TraceId: {TraceId}",
                error.StatusCode,
                httpContext.TraceIdentifier
            );
        }
        else
        {
            logger.LogWarning(
                "Request rejected with status {StatusCode}. Message: {Message}. TraceId: {TraceId}",
                error.StatusCode,
                exception.Message,
                httpContext.TraceIdentifier
            );
        }

        var problemDetails = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.Detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = error.StatusCode;
        httpContext.Response.ContentType =
            "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken
        );

        return true;
    }

    private static ErrorDescriptor MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentException argumentException =>
                new ErrorDescriptor(
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    argumentException.Message
                ),

            KeyNotFoundException notFoundException =>
                new ErrorDescriptor(
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    notFoundException.Message
                ),

            InvalidOperationException conflictException =>
                new ErrorDescriptor(
                    StatusCodes.Status409Conflict,
                    "Business rule conflict",
                    conflictException.Message
                ),

            RetryLimitExceededException or MySqlException =>
                new ErrorDescriptor(
                    StatusCodes.Status503ServiceUnavailable,
                    "Database unavailable",
                    "The database service is temporarily unavailable."
                ),

            _ =>
                new ErrorDescriptor(
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    "An unexpected error occurred."
                )
        };
    }

    private sealed record ErrorDescriptor(
        int StatusCode,
        string Title,
        string Detail
    );
}