namespace FibonacciHW.Api;

using System.ComponentModel;
using static FibonacciHW.Api.Enums.FibonacciServiceEnums;

public static class FibonacciEpDef
{
    public record GenerateFibonacciSequenceParams
    (
       [DefaultValue(0)] int Begin,
       [DefaultValue(5)] int End,
       [DefaultValue(false)] bool UseCache,
       [DefaultValue("00.00.05.000")] TimeSpan Timeout,
       [DefaultValue(100)] long MaxMemoryMb
    );

    public record GenerateFibonacciSequenceResponse(List<FiboNumber> Sequences, string StatusMsg);
}