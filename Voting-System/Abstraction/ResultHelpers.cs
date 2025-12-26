using Microsoft.AspNetCore.Mvc;
using VotingSystem.Dtos.Results;

namespace VotingSystem.Abstraction
{
    public static class ResultHelpers
    {
        public static ObjectResult ToProblem(this Result result)
        {
            if (result.IsSuccess)
                throw new InvalidOperationException(
                    "Cannot convert a successful result to a problem response.");

            var statusCode = result.Error.StatusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = "One or more errors occurred"
            };

            problemDetails.Extensions["errors"] = new[]
            {
                result.Error
            };

            return new ObjectResult(problemDetails)
            {
                StatusCode = statusCode
            };
        }
    }
}
