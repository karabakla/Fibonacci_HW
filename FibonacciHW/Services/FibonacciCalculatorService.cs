using FibonacciHW.Api;
using FibonacciHW.Api.Utils;
using FibonacciHW.Config;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Numerics;
namespace FibonacciHW.Services;

public class FibonacciCalculatorService<TKey, TValue> : IFibonacciCalculatorService<TKey, TValue> where TKey: INumber<TKey> where TValue : struct, INumber<TValue>
{
    ILogger<FibonacciCalculatorService<TKey, TValue>> _logger;

    IFibonacciCache<TKey, TValue> _cache;

    // We want to be able set the delay from outside for testing purposes.
    public TimeSpan DelayForNextFiboNumber { get; set; } = TimeSpan.FromMilliseconds(500);

    public FibonacciCalculatorService
    (
        ILogger<FibonacciCalculatorService<TKey, TValue>> logger,        
        IFibonacciCache<TKey, TValue> cache
    )
    {
        _logger = logger;
        _cache = cache;
    }

    async Task<(List<TValue> sequenceList, bool isOk, string errorMsg)> IFibonacciCalculatorService<TKey, TValue>.CalculateAsync
    (
        TKey begin,
        TKey end,
        bool useCache,
        long maxMemoryMb,
        CancellationToken ct
    )
    {
        // Now here we need to span new thread in order not to block the calling thread.
        return  await Task.Run(() => CalculateAsync_Impl(begin, end, useCache, maxMemoryMb, ct));
    }

    (List<TValue> sequenceList, bool isOk, string errorMsg) CalculateAsync_Impl
    (
        TKey begin,
        TKey end,
        bool useCache,
        long maxMemoryMb,
        CancellationToken ct
    )
    {
        void CheckAndAbortMemoryUsage(long limitInMb)
        {
            if (Utilities.GetCurrentMemoryUsageInMB() > limitInMb)
            {
                throw new InsufficientMemoryException();
            }
        }

        List<TValue> result = new();

        TValue fiboValueN_2 = TValue.Zero;
        TValue fiboValueN_1 = TValue.One;

        for (TKey idx = TKey.Zero; idx <= end; idx++)
        {
            try
            {
                if(ct.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                TValue? fiboValueN = useCache ? _cache.GetValue(idx) : null;

                if (fiboValueN is null)
                {
                    fiboValueN = GetNextFiboValue(fiboValueN_1, fiboValueN_2, ct);
                }

                (fiboValueN_2, fiboValueN_1) = (fiboValueN_1, fiboValueN.Value);

                if (useCache)
                {
                    CheckAndAbortMemoryUsage(maxMemoryMb);

                    _cache.AddOrUpdate(idx, fiboValueN.Value);
                }

                // We only want to return values after the begin
                if (idx >= begin)
                {
                    CheckAndAbortMemoryUsage(maxMemoryMb);

                    result.Add(fiboValueN.Value);
                }
            }
            catch (InsufficientMemoryException)
            {
                _logger.LogInformation("Calculation stopped due to insufficient memory");
                return (result, false, "Calculation stopped due to insufficient memory");
            }
            catch(Exception)
            {
                _logger.LogInformation("Calculation stopped due to time out");
                return (result, false, "Calculation stopped due to time out");
            }
        }

        return (result, true, "");
    }

    //f(n) = f(n-1) + f(n-2)
    TValue GetNextFiboValue(TValue fiboValueN_2, TValue fiboValueN_1, CancellationToken ct)
    {
        TValue ret = fiboValueN_2 + fiboValueN_1;

        // To represent a long calculation which is blocking the thread. should not be cannot be called async web request.
        Task.Delay(DelayForNextFiboNumber, ct).Wait();

        return ret;
    }
}
