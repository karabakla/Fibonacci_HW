
// For easyness of use, Let's set the key and value types here.
global using TFiboKey = long;
global using TFiboValue = long;

using FibonacciHW.Config;
using FibonacciHW.Services;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.IncludeFields = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.WriteIndented = true;
});

var config = builder.Configuration;

services.AddOptions();
services.AddIOptionToDI<FibonacciServiceOptions>(config);

services.AddSingleton<IFibonacciCache<TFiboKey, TFiboValue>, FibonacciCache<TFiboKey, TFiboValue>>();
services.AddSingleton<IFibonacciCalculatorService<TFiboKey, TFiboValue>, FibonacciCalculatorService<TFiboKey, TFiboValue>>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
