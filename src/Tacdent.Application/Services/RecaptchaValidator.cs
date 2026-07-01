using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Tacdent.Application.Errors;
using Tacdent.Application.Options;
using Tacdent.Application.Services.Interfaces;
using Tacdent.Core.Results;

namespace Tacdent.Application.Services;

public class RecaptchaValidator(HttpClient httpClient, IOptions<RecaptchaOptions> options) : IRecaptchaValidator
{
    public async Task<Result> ValidateAsync(
        string token,
        string expectedAction,
        string? remoteIp,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        if (!settings.Enabled || string.IsNullOrWhiteSpace(settings.SecretKey))
        {
            return Result.Success();
        }

        try
        {
            using var content = new FormUrlEncodedContent(BuildFormData(token, remoteIp, settings.SecretKey));
            using var response = await httpClient.PostAsync(settings.VerifyEndpoint, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure(RecaptchaErrors.Unavailable);
            }

            var verifyResponse = await response.Content.ReadFromJsonAsync<SiteVerifyResponse>(cancellationToken);

            if (verifyResponse is null || !verifyResponse.Success)
            {
                return Result.Failure(RecaptchaErrors.Failed);
            }

            if (!string.Equals(verifyResponse.Action, expectedAction, StringComparison.Ordinal))
            {
                return Result.Failure(RecaptchaErrors.Failed);
            }

            if (verifyResponse.Score < settings.MinScore)
            {
                return Result.Failure(RecaptchaErrors.LowScore);
            }

            return Result.Success();
        }
        catch (HttpRequestException)
        {
            return Result.Failure(RecaptchaErrors.Unavailable);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Failure(RecaptchaErrors.Unavailable);
        }
    }

    private static IEnumerable<KeyValuePair<string, string>> BuildFormData(
        string token,
        string? remoteIp,
        string secretKey)
    {
        yield return new KeyValuePair<string, string>("secret", secretKey);
        yield return new KeyValuePair<string, string>("response", token);

        if (!string.IsNullOrWhiteSpace(remoteIp))
        {
            yield return new KeyValuePair<string, string>("remoteip", remoteIp);
        }
    }

    private sealed class SiteVerifyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }
    }
}
