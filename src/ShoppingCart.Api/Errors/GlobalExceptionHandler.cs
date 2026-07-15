using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using ShoppingCart.Application.Common.Exceptions;

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
                exception,
                "Request rejected with status {StatusCode}. TraceId: {TraceId}",
                error.StatusCode,
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

    private static ErrorDescriptor MapException(
        Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException unauthorizedException =>
                new ErrorDescriptor(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    unauthorizedException.Message
                ),

            ResourceNotFoundException notFoundException =>
                new ErrorDescriptor(
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    notFoundException.Message
                ),

            KeyNotFoundException notFoundException =>
                new ErrorDescriptor(
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    notFoundException.Message
                ),

            BusinessConflictException conflictException =>
                new ErrorDescriptor(
                    StatusCodes.Status409Conflict,
                    "Business rule conflict",
                    conflictException.Message
                ),

            ArgumentException argumentException =>
                new ErrorDescriptor(
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    argumentException.Message
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