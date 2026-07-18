using Microsoft.Extensions.Options;
using Tacdent.Api.Logging;
using Tacdent.Api.Options;

namespace Tacdent.UnitTests.Api;

public class SensitiveDataRedactorTests
{
    [Fact]
    public void RedactJson_MasksConfiguredFields_CaseInsensitive()
    {
        var redactor = CreateRedactor();

        var result = redactor.RedactJson(
            """{"password":"secret","Email":"a@b.com","serviceId":1,"nested":{"phone":"555"}}""");

        result.ShouldContain("\"password\":\"***\"");
        result.ShouldContain("\"Email\":\"***\"");
        result.ShouldContain("\"phone\":\"***\"");
        result.ShouldContain("\"serviceId\":1");
        result.ShouldNotContain("secret");
        result.ShouldNotContain("a@b.com");
        result.ShouldNotContain("555");
    }

    [Fact]
    public void RedactJson_ReturnsRedactedMarker_ForInvalidJson()
    {
        var redactor = CreateRedactor();

        redactor.RedactJson("{not-json").ShouldBe("[redacted]");
    }

    private static SensitiveDataRedactor CreateRedactor()
    {
        var options = Options.Create(new RequestLoggingOptions());
        return new SensitiveDataRedactor(options);
    }
}
