
// For easy use, let's have a common Fibonacci number defined here.
global using FiboNumber = long;

using FibonacciHW.Config;
using FibonacciHW.Api;
using FibonacciHW.Services;
using Microsoft.AspNetCore.Http.Json;
using FibonacciHW.MiddleWares;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();

services.AddSwaggerGen(options =>
{
    options.MapType<TimeSpan>(() => new OpenApiSchema
    {
        Type = "string",
        Example = new OpenApiString("00:00:00.000")
    });
});

services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.IncludeFields = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.WriteIndented = true;
});

var config = builder.Configuration;

services.AddOptions();
services.AddIOptionToDI<FibonacciCacheOptions>(config);

services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

services.AddSingleton<IFibonacciCache<FiboNumber>, FibonacciCache<FiboNumber>>();
services.AddSingleton<IFibonacciCalculatorService<FiboNumber>, FibonacciCalculatorService<FiboNumber>>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapControllers();

app.Run();