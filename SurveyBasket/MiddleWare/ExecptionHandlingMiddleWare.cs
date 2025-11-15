namespace SurveyBasket.MiddleWare
{
    public class ExecptionHandlingMiddleWare(RequestDelegate next , ILogger<ExecptionHandlingMiddleWare> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExecptionHandlingMiddleWare> _logger = logger;
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the request.",ex.Message);
               var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                   Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
               };
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
