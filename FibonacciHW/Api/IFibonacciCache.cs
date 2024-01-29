using System.Numerics;

namespace FibonacciHW.Api;


// This cache may not be working on local machine maybe a Redis can be used instead.
// If a remote cache or similar is used, then we could write async methods. But for now we don't need it.
// For solid concerns, cache is separated from the calculator.
public interface IFibonacciCache<TFibonacciNumber> where TFibonacciNumber : struct, INumber<TFibonacciNumber>
{
    public TFibonacciNumber? GetValue(int key);

    public void AddOrUpdate(int key, TFibonacciNumber value);

    public bool IsEmpty { get; }
}
