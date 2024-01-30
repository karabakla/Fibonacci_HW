using FakeItEasy;

using FiboCache = FibonacciHW.Api.FibonacciCache<long>;
using IFiboCache = FibonacciHW.Api.IFibonacciCache<long>;
using FiboCalculatorService = FibonacciHW.Services.FibonacciCalculatorService<long>;
using IFiboCalculatorService = FibonacciHW.Services.IFibonacciCalculatorService<long>;

using static FibonacciHW.Api.Enums.FibonacciServiceEnums;

using Microsoft.Extensions.Logging;
using System.Diagnostics;
using FibonacciHW.Controllers;
using FibonacciHW.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FibonacciHW.Filters;
using Microsoft.AspNetCore.Http;

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

    IFiboCache CreateFiboCacheWithValues(TimeSpan invalidateCacheAfter, List<long> values)
    {
        var fiboCache = CreateFiboCache(invalidateCacheAfter);

        for (int i = 0; i < values.Count; i++)
        {
            fiboCache.AddOrUpdate(i, values[i]);
        }

        return fiboCache;
    }

    IFiboCalculatorService CreateFiboCalculatorService(IFiboCache cache, TimeSpan delayForNextFiboNumber)
    {
        var logger = A.Fake<ILogger<FiboCalculatorService>>();

        var fiboService = new FiboCalculatorService(logger, cache);

        fiboService.DelayForNextFiboNumber = delayForNextFiboNumber;

        return fiboService;
    }

    FibonacciController CreateFibonacciController(IFiboCalculatorService fiboService)
    {
        var logger = A.Fake<ILogger<FibonacciController>>();

        return new FibonacciController(logger, fiboService);
    }

    //---------------------------------------------------------------------------------------
    // Testing the cache.
    //---------------------------------------------------------------------------------------
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


    //---------------------------------------------------------------------------------------
    // Testing the calculator service without cache
    //---------------------------------------------------------------------------------------
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
    public async Task Should_Return_Empty_Sequence_With_TimeoutError_From_Fibo_Service()
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
    public async Task Should_Return_Empty_Sequence_With_MemoryError_From_Fibo_Service()
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


    //---------------------------------------------------------------------------------------
    // Testing the calculator service with cache
    //---------------------------------------------------------------------------------------
    [Fact]
    public async Task Should_Return_Sequence_Using_Cache_Without_Timeout_From_Fibo_Service()
    {
        var fiboCache = CreateFiboCacheWithValues(TimeSpan.FromSeconds(1000), FiboSequence_0_20);

        var fiboService = CreateFiboCalculatorService(fiboCache, TimeSpan.FromMilliseconds(500));

        var beginIndex = 0;
        var endIndex = 20;

        var ct = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token;

        var (sequenceList, status) = await fiboService.CalculateAsync(beginIndex, endIndex, true, long.MaxValue, ct);

        Assert.Equal(FibonacciServiceStatusCode.None, status);

        Assert.NotEmpty(sequenceList);
    }

    //---------------------------------------------------------------------------------------
    // Testing the controller
    //---------------------------------------------------------------------------------------
    [Fact]
    public async Task Should_Return_Sequence_From_Fibo_Controller()
    {
        var fiboCache = CreateFiboCache(TimeSpan.FromSeconds(1000));

        var fiboService = CreateFiboCalculatorService(fiboCache, TimeSpan.FromMilliseconds(500));

        var controller = CreateFibonacciController(fiboService);

        var requestParams = new FibonacciEpDef.GenerateFibonacciSequenceParams(0, 20, false, TimeSpan.FromSeconds(1000), 100);

        var response = await controller.Calculate(requestParams);

        Assert.IsType<OkObjectResult>(response);
    }

    [Fact]
    public async Task Should_Return_Partial_Sequence_From_Fibo_Controller()
    {
        var fiboCache = CreateFiboCache(TimeSpan.FromSeconds(1000));

        var fiboService = CreateFiboCalculatorService(fiboCache, TimeSpan.FromMilliseconds(500));

        var controller = CreateFibonacciController(fiboService);


        var requestParams = new FibonacciEpDef.GenerateFibonacciSequenceParams(0, 20, false, TimeSpan.FromMilliseconds(1500), 100);


        var response = await controller.Calculate(requestParams);

        var responseOkObj = response as OkObjectResult;

        Assert.NotNull(responseOkObj);

        var responseObj = responseOkObj.Value as FibonacciEpDef.GenerateFibonacciSequenceResponse;

        Assert.NotNull(responseObj);

        Assert.NotEmpty(responseObj.Sequences);

        Assert.NotEmpty(responseObj.StatusMsg);
    }
}
