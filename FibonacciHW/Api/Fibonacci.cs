namespace FibonacciHW.Api;

using FiboValueList = List<TFiboValue>;
public static class FibonacciDefs
{
    public record Request
    (
        int Begin,
        int End,
        bool UseCache,
        int TimeoutMs,
        int MaxMemoryMb
    );

    public record Response
    {
        public string ErrorMsg { get; init; } = string.Empty;
        public FiboValueList Sequences { get; init; } = new();

        public static Response Success(FiboValueList sequences) => new() { Sequences = sequences };
        public static Response Fail(FiboValueList sequences, string errorMsg) => new() { Sequences = sequences, ErrorMsg = errorMsg };
    }
}
