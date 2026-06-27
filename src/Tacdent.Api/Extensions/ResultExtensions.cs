using Microsoft.AspNetCore.Mvc;
using Tacdent.Core.Results;

namespace Tacdent.Api.Extensions;

/// <summary>
/// Translates an Application <see cref="Result"/> into an HTTP response, mapping
/// <see cref="ErrorType"/> to the appropriate status code.
/// </summary>
public static class ResultExtensions
{
    public static IActionResult ToOkResult<TValue>(this Result<TValue> result, Func<TValue, object> map)
        => result.IsSuccess
            ? new OkObjectResult(map(result.Value))
            : result.Error.ToProblemResult();

    public static IActionResult ToNoContentResult(this Result result)
        => result.IsSuccess
            ? new NoContentResult()
            : result.Error.ToProblemResult();

    public static IActionResult ToCreatedResult<TValue>(this Result<TValue> result, Func<TValue, object> map)
        => result.IsSuccess
            ? new CreatedResult(string.Empty, map(result.Value))
            : result.Error.ToProblemResult();

    public static IActionResult ToProblemResult(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ObjectResult(new { code = error.Code, message = error.Message })
        {
            StatusCode = statusCode
        };
    }
}
