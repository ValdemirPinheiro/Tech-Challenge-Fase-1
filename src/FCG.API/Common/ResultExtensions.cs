using FCG.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
        => result.IsSuccess
            ? new NoContentResult()
            : MapError(result.Error!, result.ErrorType);

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess) return new OkObjectResult(result.Value);
        return MapError(result.Error!, result.ErrorType);
    }

    public static IActionResult ToCreatedActionResult<T>(this Result<T> result, string? location = null)
    {
        if (!result.IsSuccess) return MapError(result.Error!, result.ErrorType);
        return new ObjectResult(result.Value) { StatusCode = StatusCodes.Status201Created };
    }

    private static IActionResult MapError(string error, ErrorType type)
    {
        var status = type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Title = type.ToString(),
            Detail = error,
            Status = status
        };

        return new ObjectResult(problem) { StatusCode = status };
    }
}
