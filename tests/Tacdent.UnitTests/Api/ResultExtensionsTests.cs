using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tacdent.Api.Extensions;
using Tacdent.Core.Results;

namespace Tacdent.UnitTests.Api;

public class ResultExtensionsTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError)]
    public void ToProblemResult_MapsErrorTypeToStatusCode(ErrorType errorType, int expectedStatusCode)
    {
        var error = new Error("Test.Error", "Test message.", errorType);

        var result = error.ToProblemResult() as ObjectResult;

        result.ShouldNotBeNull();
        result!.StatusCode.ShouldBe(expectedStatusCode);
    }

    [Fact]
    public void ToOkResult_WhenSuccess_ReturnsOkWithMappedValue()
    {
        var result = Result.Success("value");

        var actionResult = result.ToOkResult(v => new { Value = v }) as OkObjectResult;

        actionResult.ShouldNotBeNull();
        actionResult!.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Fact]
    public void ToOkResult_WhenFailure_ReturnsProblemResult()
    {
        var result = Result.Failure<string>(Error.NotFound("Test.NotFound", "Missing."));

        var actionResult = result.ToOkResult(v => v) as ObjectResult;

        actionResult.ShouldNotBeNull();
        actionResult!.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void ToNoContentResult_WhenSuccess_ReturnsNoContent()
    {
        var result = Result.Success();

        var actionResult = result.ToNoContentResult();

        actionResult.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public void ToNoContentResult_WhenFailure_ReturnsProblemResult()
    {
        var result = Result.Failure(Error.Validation("Test.Validation", "Invalid."));

        var actionResult = result.ToNoContentResult() as ObjectResult;

        actionResult.ShouldNotBeNull();
        actionResult!.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }
}
