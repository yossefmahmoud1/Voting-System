using Microsoft.AspNetCore.Diagnostics;

namespace SurveyBasket.Errors
{
    public class GlobalExecptionHandler (ILogger<GlobalExecptionHandler> logger): IExceptionHandler
    {
        private readonly ILogger<GlobalExecptionHandler> _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred while processing the request.", exception.Message);
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
            };
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails);
            return true;
        }
    }
}
