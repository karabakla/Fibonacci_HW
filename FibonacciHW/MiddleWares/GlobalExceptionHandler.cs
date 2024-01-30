using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FibonacciHW.MiddleWares;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    async ValueTask<bool> IExceptionHandler.TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Lets log the exception here, so we can later on investigate it from logs.
        _logger.LogError($"Unhandled exception occurred: {exception}");

        // We don't want to expose the exception details to the client, just the message so that we can keep track it from logs.
        var problemDetails = new ProblemDetails
        {
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
        return true;
    }
}
