using Microsoft.AspNetCore.Mvc;
using AppResult = FCG.Application.Common.Result;
using AppErrorType = FCG.Application.Common.ErrorType;

namespace FCG.API.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this AppResult result)
        => result.IsSuccess
            ? new NoContentResult()
            : MapError(result.Error!, result.ErrorType);

    public static IActionResult ToActionResult<T>(this FCG.Application.Common.Result<T> result)
    {
        if (result.IsSuccess) return new OkObjectResult(result.Value);
        return MapError(result.Error!, result.ErrorType);
    }

    public static IActionResult ToCreatedActionResult<T>(this FCG.Application.Common.Result<T> result, string? location = null)
    {
        if (!result.IsSuccess) return MapError(result.Error!, result.ErrorType);
        return new ObjectResult(result.Value) { StatusCode = StatusCodes.Status201Created };
    }

    private static IActionResult MapError(string error, AppErrorType type)
    {
        var status = type switch
        {
            AppErrorType.NotFound => StatusCodes.Status404NotFound,
            AppErrorType.Conflict => StatusCodes.Status409Conflict,
            AppErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            AppErrorType.Forbidden => StatusCodes.Status403Forbidden,
            AppErrorType.Validation => StatusCodes.Status400BadRequest,
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
