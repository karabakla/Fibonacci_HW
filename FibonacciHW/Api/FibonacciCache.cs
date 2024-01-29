using FibonacciHW.Config;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Numerics;
using System.Timers;

namespace FibonacciHW.Api;

public class FibonacciCache<TFibonacciNumber> : IFibonacciCache<TFibonacciNumber> where TFibonacciNumber : struct, INumber<TFibonacciNumber>
{

    private readonly ILogger<FibonacciCache<TFibonacciNumber>> _logger;

    private readonly ConcurrentDictionary<int, TFibonacciNumber> _cache;

    private readonly System.Timers.Timer _timer;

    public FibonacciCache
    (
        ILogger<FibonacciCache<TFibonacciNumber>> logger,
        IOptions<FibonacciCacheOptions> config
    )
    {
        _logger = logger;
        _cache = new();

        _timer = new System.Timers.Timer(config.Value.InvalidateCacheAfter);
        _timer.Elapsed += OnTimer;
    }

    TFibonacciNumber? IFibonacciCache<TFibonacciNumber>.GetValue(int index)
    {
        // Cache in use so reset the timer.
        _timer.Stop();
        _timer.Start();

        if (_cache.TryGetValue(index, out var value))
        {
            return value;
        }

        return null;
    }

    void IFibonacciCache<TFibonacciNumber>.AddOrUpdate(int index, TFibonacciNumber value)
    {
        // Cache in use so reset the timer.
        _timer.Stop();
        _timer.Start();

        _cache[index] = value;
    }

    bool IFibonacciCache<TFibonacciNumber>.IsEmpty => _cache.IsEmpty;

    //--------------------------------------------------------------------------------------- 
    // Clear the cache after a period of time.
    //---------------------------------------------------------------------------------------
    void OnTimer(object? sender, ElapsedEventArgs e)
    {
        _cache.Clear();
        _timer.Stop();
        _logger.LogInformation("OnTimer");
    }
}
