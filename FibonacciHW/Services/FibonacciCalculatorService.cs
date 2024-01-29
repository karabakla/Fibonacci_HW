using FibonacciHW.Api;
using System.Diagnostics;
using System.Numerics;
using static FibonacciHW.Api.Enums.FibonacciServiceEnums;
namespace FibonacciHW.Services;

public class FibonacciCalculatorService<TFibonacciNumber> : IFibonacciCalculatorService<TFibonacciNumber> where TFibonacciNumber : struct, INumber<TFibonacciNumber>
{
    ILogger<FibonacciCalculatorService<TFibonacciNumber>> _logger;

    IFibonacciCache<TFibonacciNumber> _cache;

    // We want to be able set the delay from outside for testing purposes.
    public TimeSpan DelayForNextFiboNumber { get; set; } = TimeSpan.FromMilliseconds(500);

    public FibonacciCalculatorService
    (
        ILogger<FibonacciCalculatorService<TFibonacciNumber>> logger,
        IFibonacciCache<TFibonacciNumber> cache
    )
    {
        _logger = logger;
        _cache = cache;
    }

    async Task<(List<TFibonacciNumber> sequenceList, FibonacciServiceStatusCode status)> IFibonacciCalculatorService<TFibonacciNumber>.CalculateAsync
    (
        int begin,
        int end,
        bool useCache,
        long memoryLimitInBytes,
        CancellationToken ct
    )
    {
        // Now here we need to span new thread in order not to block the calling thread.
        return await Task.Run(() => CalculateAsync_Impl(begin, end, useCache, memoryLimitInBytes, ct));
    }

    (List<TFibonacciNumber> sequenceList, FibonacciServiceStatusCode status) CalculateAsync_Impl
    (
        int begin,
        int end,
        bool useCache,
        long memoryLimitInBytes,
        CancellationToken ct
    )
    {
        List<TFibonacciNumber> result = new();

        TFibonacciNumber fiboValueN_2 = TFibonacciNumber.Zero;
        TFibonacciNumber fiboValueN_1 = TFibonacciNumber.One;
        TFibonacciNumber? fiboValueN;

        for (int idx = 0; idx <= end; idx++)
        {
            try
            {
                if (Process.GetCurrentProcess().PrivateMemorySize64 > memoryLimitInBytes)
                {
                    throw new InsufficientMemoryException();
                }

                if (ct.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                fiboValueN = useCache ? _cache.GetValue(idx) : null;

                if (fiboValueN is null)
                {
                    if (idx < 2)
                    {
                        //f(0) = 0
                        //f(1) = 1
                        fiboValueN = idx == 0 ? TFibonacciNumber.Zero : TFibonacciNumber.One;
                    }
                    else
                    {
                        //f(n) = f(n-1) + f(n-2)
                        fiboValueN = fiboValueN_2 + fiboValueN_1;
                    }

                    SimulateLongRunningTaskWait(DelayForNextFiboNumber, ct);
                }

                (fiboValueN_2, fiboValueN_1) = (fiboValueN_1, fiboValueN.Value);

                if (useCache)
                {
                    _cache.AddOrUpdate(idx, fiboValueN.Value);
                }

                // We only want to return values after the begin
                if (idx >= begin)
                {
                    result.Add(fiboValueN.Value);
                }
            }
            catch (InsufficientMemoryException)
            {
                _logger.LogInformation("Calculation stopped due to insufficient memory");
                return (result, FibonacciServiceStatusCode.OutOfMemory);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Calculation stopped due to time out");
                return (result, FibonacciServiceStatusCode.Timeout);
            }
        }

        return (result, FibonacciServiceStatusCode.None);
    }


    void SimulateLongRunningTaskWait(TimeSpan amount, CancellationToken ct)
    {
        void MicroSecondsDelay(long us)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long v = (us * System.Diagnostics.Stopwatch.Frequency) / 1000000;
            while (sw.ElapsedTicks < v) ;
        }

        // To represent a long calculation which is blocking the thread. should not be cannot be called async web request.
        var divideAmount = 1000;
        long granularWaitTimeInUs = (long)Math.Max(amount.TotalMicroseconds / divideAmount, 1);
        for (int i = 0; i < divideAmount; i++)
        {
            MicroSecondsDelay(granularWaitTimeInUs);
            if (ct.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
        }
    }
}
