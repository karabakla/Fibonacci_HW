namespace FibonacciHW.Api;

using Microsoft.AspNetCore.Mvc;
using static FibonacciHW.Api.Enums.FibonacciServiceEnums;
using FiboValueList = List<FiboNumber>;

public static class FibonacciEpDef
{
    // Used as the numeric status code in custom HTTP problem details
    public enum StatusCodes
    {
        None,
        InvalidTimeoutValue,
        InvalidBeginValue,
        InvalidEndValue,
        InvalidBeginEndValues,
        InvalidTimeoutValues,
        InvalidMaxMemoryValue,
        Timeout,
        OutOfMemory,

        Unknown
    }

    public record Request
    (
        int Begin,
        int End,
        bool UseCache,
        TimeSpan Timeout,
        long MaxMemoryMb
    );

    // Extend problem details with ErrorCode and ErrorMsg as needed.
    public record FibonacciProblemDetails(StatusCodes StatusCode, string ErrorMsg)
    {
        public static FibonacciProblemDetails Nil => new(StatusCodes.None, string.Empty);

        public static FibonacciProblemDetails From(FibonacciServiceStatusCode error) =>
            error switch
            {
                FibonacciServiceStatusCode.Timeout => new(StatusCodes.Timeout, "Timeout"),
                FibonacciServiceStatusCode.OutOfMemory => new(StatusCodes.OutOfMemory, "Out of memory"),
                _ => new(StatusCodes.Unknown, "Unknown error, please contact to management")
            };
    }

    public record Response
    {
        public FibonacciProblemDetails ProblemDetails { get; init; } = FibonacciProblemDetails.Nil;
        public FiboValueList Sequences { get; init; } = new();

        public bool IsSuccessful => ProblemDetails.StatusCode == StatusCodes.None;
        
        public static Response Success(FiboValueList sequences) => new() { Sequences = sequences };
        public static Response Fail(FiboValueList sequences, FibonacciProblemDetails problemDetails) => new() { Sequences = sequences, ProblemDetails = problemDetails };
        public static Response Fail(FibonacciProblemDetails problemDetails) => new() { ProblemDetails = problemDetails };
    }

    public static bool ValidateRequest(this Request request, out FibonacciProblemDetails problemDetails)
    {
        if (request.Begin < 0)
        {
            problemDetails = new(StatusCodes.InvalidBeginValue, "Begin must be greater than or equal to zero.");
            return false;
        }

        if (request.End < 0)
        {
            problemDetails = new(StatusCodes.InvalidEndValue, "End must be greater than or equal to zero.");
            return false;
        }

        if (request.Begin > request.End)
        {
            problemDetails = new(StatusCodes.InvalidBeginEndValues, "Begin must be less than or equal to End.");
            return false;
        }

        if (request.Begin == request.End)
        {
            problemDetails = new(StatusCodes.InvalidBeginEndValues, "Begin must be different than End.");
            return false;
        }

        if (request.Timeout < TimeSpan.Zero)
        {
            problemDetails = new(StatusCodes.InvalidTimeoutValue, "TimeoutMs must be greater than or equal to zero.");
            return false;
        }

        if (request.MaxMemoryMb < 0)
        {
            problemDetails = new(StatusCodes.InvalidBeginEndValues, "MaxMemoryMb must be greater than or equal to zero.");

            return false;
        }

        problemDetails = FibonacciProblemDetails.Nil;
        return true;
    }
}
