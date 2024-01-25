using FibonacciHW.Api;
using System.Numerics;

namespace FibonacciHW.Services;

public interface IFibonacciCalculatorService<TKey, TValue> where TKey : INumber<TKey> where TValue : struct, INumber<TValue>
{
    public Task<(List<TValue> sequenceList, bool isOk, string errorMsg)> CalculateAsync(TKey begin, TKey end, bool useCache, long maxMemoryMb, CancellationToken ct);
}
