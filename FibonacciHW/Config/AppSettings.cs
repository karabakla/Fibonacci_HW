namespace FibonacciHW.Config;


public class FibonacciServiceOptions
{
    public TimeSpan InvalidateCacheAfter { get; set; }
}


public static class OptionExtensions
{
    public static TOptions AddIOptionToDI<TOptions>(this IServiceCollection services, IConfiguration config) where TOptions : class, new()
    {
        var sectionName = typeof(TOptions).Name;

        services.Configure<TOptions>(setting =>
        {
            config.GetSection(sectionName).Bind(setting);
        });
        var oTOptions = new TOptions();
        config.GetSection(sectionName).Bind(oTOptions);

        return oTOptions;
    }
}