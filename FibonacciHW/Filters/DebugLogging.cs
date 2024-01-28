using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace FibonacciHW.Filters;

public class DebugLogging : ActionFilterAttribute, IAsyncActionFilter
{
    private ILogger<DebugLogging> _logger = default!;
    async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
#if DEBUG
        var logger = GetOrCreateLogger(context);

        logger.LogInformation($"Action {context.ActionDescriptor.DisplayName} is executing");

        var sw = Stopwatch.StartNew();

        await next();

        sw.Stop();
        logger.LogInformation($"Action {context.ActionDescriptor.DisplayName} executed in {sw.ElapsedMilliseconds} ms" );
#endif
        await next();
    }


    public ILogger<DebugLogging> GetOrCreateLogger(ActionExecutingContext context)
    {
        if (_logger is not null)
        {
            return _logger;
        }
        var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();

        var logger = loggerFactory?.CreateLogger<DebugLogging>();

        Debug.Assert(logger is not null);

        _logger = logger;
        return _logger;
    }
}
