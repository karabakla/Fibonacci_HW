using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Timers;

namespace FibonacciHW.Services;

public interface IFibonacciCache<TKey, TValue> where TKey : INumber<TKey> where TValue : struct, INumber<TValue>
{
    public TValue? GetValue(TKey key);

    public void AddOrUpdate(TKey key, TValue value);
}
