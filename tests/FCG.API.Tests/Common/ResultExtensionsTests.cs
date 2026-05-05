using FCG.API.Common;
using FCG.Application.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Tests.Common;

public class ResultExtensionsTests
{
    [Fact]
    public void Failure_ComNotFound_DeveMapearPara404()
    {
        var result = Result<string>.Failure("não existe", ErrorType.NotFound);
        var action = result.ToActionResult() as ObjectResult;

        action.Should().NotBeNull();
        action!.StatusCode.Should().Be(404);
    }

    [Fact]
    public void Failure_ComConflict_DeveMapearPara409()
    {
        var result = Result.Failure("duplicado", ErrorType.Conflict);
        var action = result.ToActionResult() as ObjectResult;

        action.Should().NotBeNull();
        action!.StatusCode.Should().Be(409);
    }

    [Fact]
    public void Success_DeveDevolver200()
    {
        var result = Result<string>.Success("ok");
        var action = result.ToActionResult() as OkObjectResult;
        action.Should().NotBeNull();
        action!.Value.Should().Be("ok");
    }

    [Fact]
    public void Created_DeveDevolver201()
    {
        var result = Result<string>.Success("ok");
        var action = result.ToCreatedActionResult() as ObjectResult;
        action.Should().NotBeNull();
        action!.StatusCode.Should().Be(201);
    }
}
