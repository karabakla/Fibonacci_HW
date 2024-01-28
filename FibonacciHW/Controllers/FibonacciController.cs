using FibonacciHW.Api;
using FibonacciHW.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace FibonacciHW.Controllers;

[ApiController]
[Route("[controller]")]
public class FibonacciController : ControllerBase
{
    private readonly ILogger<FibonacciController> _logger;
    private readonly IFibonacciCalculatorService<TFiboKey, TFiboValue> _fibonacciCalculator;
    public FibonacciController
    (
        ILogger<FibonacciController> logger,
        IFibonacciCalculatorService<TFiboKey, TFiboValue> fibonacciCalculator
    )
    {
        _logger = logger;
        _fibonacciCalculator = fibonacciCalculator;
    }

    [HttpPost(Name = "Calculate")]
    public async Task<ActionResult> Calculate(FibonacciDefs.Request request)
    {
        var cts = new CancellationTokenSource(request.TimeoutMs);

        var (sequenceList, isOk, errorMsg) =
            await _fibonacciCalculator.CalculateAsync
            (
                request.Begin,
                request.End,
                request.UseCache,
                request.MaxMemoryMb,
                cts.Token
            );

        if (isOk)
        {
            Debug.Assert(sequenceList is not null);
            return Ok(FibonacciDefs.Response.Success(sequenceList));
        }

        return Ok(FibonacciDefs.Response.Fail(sequenceList, errorMsg));
    }
}
