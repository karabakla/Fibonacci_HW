using FibonacciHW.Api;
using FibonacciHW.Api.Enums;
using FibonacciHW.Filters;
using FibonacciHW.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace FibonacciHW.Controllers;

[ApiController]
[Route("[controller]")]
public class FibonacciController : ControllerBase
{
    private readonly ILogger<FibonacciController> _logger;
    private readonly IFibonacciCalculatorService<FiboNumber> _fibonacciCalculator;
    public FibonacciController
    (
        ILogger<FibonacciController> logger,
        IFibonacciCalculatorService<FiboNumber> fibonacciCalculator
    )
    {
        _logger = logger;
        _fibonacciCalculator = fibonacciCalculator;
    }

    [DebugLogging]
    [HttpPost("Calculate")]
    public async Task<ActionResult> Calculate(FibonacciEpDef.Request request)
    {
        if (!request.ValidateRequest(out var validationError))
        {
            // We can use Response Fail here, but let's use BadRequest for now.
            return BadRequest(FibonacciEpDef.Response.Fail(validationError));
        }

        var cts = new CancellationTokenSource(request.Timeout);

        var (sequenceList, status) = await _fibonacciCalculator.CalculateAsync
        (
            request.Begin,
            request.End,
            request.UseCache,
            request.MaxMemoryMb.MegaBytesToBytes(),
            cts.Token
        );

        if (status == FibonacciServiceEnums.FibonacciServiceStatusCode.None)
        {
            Debug.Assert(sequenceList is not null);
            return Ok(FibonacciEpDef.Response.Success(sequenceList));
        }

        var problemDetails = FibonacciEpDef.FibonacciProblemDetails.From(status);

        return Ok(FibonacciEpDef.Response.Fail(sequenceList, problemDetails));
    }
}
