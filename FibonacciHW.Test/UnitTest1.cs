using FakeItEasy;

using FiboCache = FibonacciHW.Api.FibonacciCache<long>;
using IFiboCache = FibonacciHW.Api.IFibonacciCache<long>;
using FiboCalculatorService = FibonacciHW.Services.FibonacciCalculatorService<long>;
using IFiboCalculatorService = FibonacciHW.Services.IFibonacciCalculatorService<long>;

using static FibonacciHW.Api.Enums.FibonacciServiceEnums;

using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FibonacciHW.Test;
public class FibonacciHW_Tests
{
    //---------------------------------------------------------------------------------------
    // Prep methods
    //---------------------------------------------------------------------------------------

    static readonly List<long> FiboSequence_0_20 = new() { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181, 6765 };
    static readonly List<long> FiboSequence_10_20 = FiboSequence_0_20.Skip(10).ToList();

    IFiboCache CreateFiboCache(TimeSpan invalidateCacheAfter)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new Config.FibonacciCacheOptions()
        {
            InvalidateCacheAfter = invalidateCacheAfter
        });

        var logger = A.Fake<ILogger<FiboCache>>();

        return new FiboCache(logger, options);
    }

    IFiboCalculatorService CreateFiboCalculatorService(IFiboCache cache, TimeSpan delayForNextFiboNumber)
    {
        var logger = A.Fake<ILogger<FiboCalculatorService>>();

        var fiboService = new FiboCalculatorService(logger, cache);

        fiboService.DelayForNextFiboNumber = delayForNextFiboNumber;

        return fiboService;
    }


    //---------------------------------------------------------------------------------------
    /// <summary>
    /// Testing the cache.
    /// </summary>
    [Fact]
    public void Should_Return_Cached_Value()
    {
        var fiboCache = CreateFiboCache(TimeSpan.FromSeconds(1000));

        for (int i = 0; i < 1000; i++)
        {
            fiboCache.AddOrUpdate(i, i);
        }

        for (int i = 0; i < 1000; i++)
        {
            var value = fiboCache.GetValue(i);
            Assert.Equal(i, value);
        }
    }

    [Fact]
    public void Should_Invalidate_Cache_After_Some_Amount_Of_Time()
    {
        var invalidateCacheAfter = TimeSpan.FromSeconds(5);

        var fiboCache = CreateFiboCache(invalidateCacheAfter);

        for (int i = 0; i < 1000; i++)
        {
            fiboCache.AddOrUpdate(i, i);
        }

        Assert.False(fiboCache.IsEmpty);

        // Add some delay to make sure the cache is invalidated.
        Thread.Sleep(invalidateCacheAfter + TimeSpan.FromMilliseconds(100));

        Assert.True(fiboCache.IsEmpty);
    }


    /// <summary>
    /// Testing the calculator service without cache
    /// </summary>
    [Fact]
    public async Task Should_Return_Sequence_From_Fibo_Service()
    {
        var fiboCache = A.Fake<IFiboCache>();
        var fiboService = CreateFiboCalculatorService(fiboCache, TimeSpan.FromMilliseconds(1));

        var beginIndex = 0;
        var endIndex = 20;

        var (sequenceList, status) = await fiboService.CalculateAsync(beginIndex, endIndex, false, long.MaxValue, CancellationToken.None);

        Assert.Equal(FibonacciServiceStatusCode.None, status);

        Assert.Equal(FiboSequence_0_20, sequenceList);
    }

    [Fact]
    public async Task Should_Return_Partial_Sequence_From_Fibo_Service()
    {
        var fiboCache = A.Fake<IFiboCache>();
        var fiboService = CreateFiboCalculatorService(fiboCache, TimeSpan.FromMilliseconds(1));

        var beginIndex = 10;
        var endIndex = 20;

        var (sequenceList, status) = await fiboService.CalculateAsync(beginIndex, endIndex, false, long.MaxValue, CancellationToken.None);

        Assert.Equal(FibonacciServiceStatusCode.None, status);

        Assert.Equal(FiboSequence_10_20, sequenceList);
    }

    [Fact]
    public async Task Should_Return_No_Sequence_With_TimeoutError_From_Fibo_Service()
    {
        var fiboCache = A.Fake<IFiboCache>();
        var fiboService = CreateFiboCalculatorService(fiboCache, TimeSpan.FromMilliseconds(500));

        var beginIndex = 0;
        var endIndex = 20;

        var ct = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)).Token;

        var (sequenceList, status) = await fiboService.CalculateAsync(beginIndex, endIndex, false, long.MaxValue, ct);

        Assert.Equal(FibonacciServiceStatusCode.Timeout, status);

        Assert.Empty(sequenceList);
    }

    [Fact]
    public async Task Should_Return_No_Sequence_With_MemoryError_From_Fibo_Service()
    {
        var fiboCache = A.Fake<IFiboCache>();
        var fiboService = CreateFiboCalculatorService(fiboCache, TimeSpan.FromMilliseconds(1));

        var beginIndex = 0;
        var endIndex = 20;

        var (sequenceList, status) = await fiboService.CalculateAsync(beginIndex, endIndex, false, 100, CancellationToken.None);

        Assert.Equal(FibonacciServiceStatusCode.MemoryLimit, status);

        Assert.Empty(sequenceList);
    }

    [Fact]
    public async Task Should_Return_Partial_Sequence_With_TimeoutError_From_Fibo_Service()
    {
        var fiboCache = A.Fake<IFiboCache>();
        var fiboService = CreateFiboCalculatorService(fiboCache, TimeSpan.FromMilliseconds(50));

        var beginIndex = 0;
        var endIndex = 20;

        var ct = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token;

        var (sequenceList, status) = await fiboService.CalculateAsync(beginIndex, endIndex, false, long.MaxValue, ct);

        Assert.Equal(FibonacciServiceStatusCode.Timeout, status);

        Assert.NotEmpty(sequenceList);
    }

    [Fact]
    public async Task Should_Return_Partial_Sequence_With_MemoryError_From_Fibo_Service()
    {
        var fiboCache = A.Fake<IFiboCache>();
        var fiboService = CreateFiboCalculatorService(fiboCache, TimeSpan.FromMilliseconds(1));

        var beginIndex = 0;
        var endIndex = int.MaxValue;

        long memoryUsageLimit = Process.GetCurrentProcess().PrivateMemorySize64 + 1024 * 1024;


        var (sequenceList, status) = await fiboService.CalculateAsync(beginIndex, endIndex, false, memoryUsageLimit, CancellationToken.None);

        Assert.Equal(FibonacciServiceStatusCode.MemoryLimit, status);

        Assert.NotEmpty(sequenceList);
    }

    [Fact]
    public async Task Should_Return_Partial_Sequence_With_Error_From_Fibo_Service()
    {
        var fiboCache = CreateFiboCache(TimeSpan.FromSeconds(1000));
        var fiboService = CreateFiboCalculatorService(fiboCache, TimeSpan.FromMilliseconds(1));

        var beginIndex = 0;
        var endIndex = 0;

        long memoryUsageLimit = Process.GetCurrentProcess().PrivateMemorySize64 + 1024 * 1024;

        var (sequenceList, status) = await fiboService.CalculateAsync(beginIndex, endIndex, false, memoryUsageLimit, CancellationToken.None);

        Assert.Equal(FibonacciServiceStatusCode.MemoryLimit, status);

        Assert.NotEmpty(sequenceList);
    }
}