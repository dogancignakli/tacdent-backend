using Tacdent.Core.Results;

namespace Tacdent.Application.Services.Interfaces;

public interface IRecaptchaValidator
{
    Task<Result> ValidateAsync(
        string token,
        string expectedAction,
        string? remoteIp,
        CancellationToken cancellationToken = default);
}
