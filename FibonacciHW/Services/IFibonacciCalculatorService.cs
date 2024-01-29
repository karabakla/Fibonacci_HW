using System.Numerics;
using static FibonacciHW.Api.Enums.FibonacciServiceEnums;

namespace FibonacciHW.Services;

public interface IFibonacciCalculatorService<TFibonacciNumber> where TFibonacciNumber : struct, INumber<TFibonacciNumber>
{
    /// <summary>
    /// Compute a specified range of Fibonacci sequence.
    /// </summary>
    /// <param name="begin">Begin Index</param>
    /// <param name="end">End Index</param>
    /// <param name="useCache">Use caching</param>
    /// <param name="memoryLimitInBytes">Maximum memory limit for current process</param>
    /// <param name="ct">Timeout source for current overall operation</param>
    /// <returns></returns>
    public Task<(List<TFibonacciNumber> sequenceList, FibonacciServiceStatusCode status)> 
    CalculateAsync
    (
        int begin, 
        int end, 
        bool useCache, 
        long memoryLimitInBytes, 
        CancellationToken ct
    );
}
