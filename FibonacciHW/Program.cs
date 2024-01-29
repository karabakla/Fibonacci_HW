global using FiboNumber = long;

using FibonacciHW.Config;
using FibonacciHW.Api;
using FibonacciHW.Services;
using Microsoft.AspNetCore.Http.Json;
using FibonacciHW.MiddleWares;

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