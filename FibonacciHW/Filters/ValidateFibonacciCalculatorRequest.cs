using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using static FibonacciHW.Api.FibonacciEpDef;

namespace FibonacciHW.Filters;

public class ValidateFibonacciCalculatorRequestAttribute : ActionFilterAttribute, IAsyncActionFilter
{
    async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {

        if (!context.ActionArguments.TryGetValue("request", out var request))
        {
            context.Result = new BadRequestObjectResult("Empty request");
            return;
        }
        
        var requestObj = request as GenerateFibonacciSequenceParams;

        if (requestObj is null)
        {
            context.Result = new BadRequestObjectResult("Invalid request");
            return;
        }

        if (!ValidateRequest(requestObj, out var validationResult))
        {
            context.Result = new BadRequestObjectResult(validationResult);
            return;
        }


        await next();
    }


    public bool ValidateRequest(GenerateFibonacciSequenceParams request, out List<string> validationResult)
    {
        validationResult = new List<string>();

        if (request.Begin < 0)
        {
            validationResult.Add("Begin must be greater than or equal to zero.");
        }

        if (request.End < 0)
        {
            validationResult.Add("End must be greater than or equal to zero.");
        }

        if (request.Begin > request.End)
        {
            validationResult.Add("Begin must be less than or equal to End.");
        }

        if (request.Begin == request.End)
        {
            validationResult.Add("Begin must be different than End.");
        }

        if (request.Timeout < TimeSpan.Zero)
        {
            validationResult.Add("Timeout must be greater than or equal to zero.");
        }

        if (request.MaxMemoryMb < 0)
        {
            validationResult.Add("MaxMemoryMb must be greater than or equal to zero.");
        }

        return validationResult.Count == 0;
    }
}
