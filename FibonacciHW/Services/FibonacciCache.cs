using FibonacciHW.Config;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Timers;

namespace FibonacciHW.Services;

public class FibonacciCache<TKey, TValue> : IFibonacciCache<TKey, TValue> where TKey : INumber<TKey> where TValue : struct, INumber<TValue>
{
    ILogger<FibonacciCache<TKey, TValue>> _logger;

    ConcurrentDictionary<TKey, TValue> _cache;

    System.Timers.Timer _timer;

    public FibonacciCache
    (
        ILogger<FibonacciCache<TKey, TValue>> logger,
        IOptions<FibonacciServiceOptions> config
    )
    {
        _logger = logger;
        _cache = new ConcurrentDictionary<TKey, TValue>();

        _timer  = new System.Timers.Timer(config.Value.InvalidateCacheAfter);
        _timer.Elapsed += OnTimer;       
    }

    public TValue? GetValue(TKey key)
    {
        // Cache in use so reset the timer.
        _timer.Stop();
        _timer.Start();

        if(_cache.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }

    public void AddOrUpdate(TKey key, TValue value)
    {
        // Cache in use so reset the timer.
        _timer.Stop();
        _timer.Start();

        _cache[key] = value;
    }

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
