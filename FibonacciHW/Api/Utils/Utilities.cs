using System.Diagnostics;

namespace FibonacciHW.Api.Utils;

public static class Utilities
{
    public static long GetCurrentMemoryUsageInMB()
    {
        var cp = Process.GetCurrentProcess();

        return cp.PrivateMemorySize64 / (1024 * 1024);
    }
}
