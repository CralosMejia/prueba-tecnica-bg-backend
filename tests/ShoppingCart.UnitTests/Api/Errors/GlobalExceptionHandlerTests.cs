using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using ShoppingCart.Api.Errors;

namespace ShoppingCart.UnitTests.Api.Errors;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsUnexpected_Returns500()
    {
        // Arrange
        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance
        );

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new Exception(
            "Sensitive internal information"
        );

        // Act
        var handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None
        );

        // Assert
        Assert.True(handled);
        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            context.Response.StatusCode
        );

        context.Response.Body.Position = 0;

        var problem = await JsonSerializer
            .DeserializeAsync<ProblemDetails>(
                context.Response.Body,
                new JsonSerializerOptions(
                    JsonSerializerDefaults.Web
                )
            );

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal(
            "Internal server error",
            problem.Title
        );

        Assert.DoesNotContain(
            "Sensitive internal information",
            problem.Detail
        );
    }
}