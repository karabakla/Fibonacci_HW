namespace FibonacciHW.Api;

public static class MathExt
{
    public static long MegaBytesToBytes(this long megaBytes)
    {
        return megaBytes * 1024 * 1024;
    }
}
