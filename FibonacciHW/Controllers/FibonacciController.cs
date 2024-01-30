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

    [HttpPost("Calculate")]
    [ValidateFibonacciCalculatorRequest]
    [ProducesResponseType<FibonacciEpDef.GenerateFibonacciSequenceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<FibonacciEpDef.GenerateFibonacciSequenceResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Calculate(FibonacciEpDef.GenerateFibonacciSequenceParams request)
    {
        var cts = new CancellationTokenSource(request.Timeout);

        var (sequenceList, status) = await _fibonacciCalculator.CalculateAsync
        (
            request.Begin,
            request.End,
            request.UseCache,
            request.MaxMemoryMb.MegaBytesToBytes(),
            cts.Token
        );

        string statusMsg = status switch
        {
            FibonacciServiceEnums.FibonacciServiceStatusCode.Timeout => "Timeout",
            FibonacciServiceEnums.FibonacciServiceStatusCode.MemoryLimit => "Memory Limit Exceeded",
            _ => string.Empty
        };

        if (sequenceList.Count == 0)
        {
            return BadRequest(statusMsg);
        }

        var response = new FibonacciEpDef.GenerateFibonacciSequenceResponse(sequenceList, statusMsg);
        return Ok(response);
    }
}
