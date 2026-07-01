using Tacdent.Core.Results;

namespace Tacdent.Application.Errors;

public static class RecaptchaErrors
{
    public static readonly Error Failed = Error.Validation(
        "Recaptcha.Failed",
        "reCAPTCHA verification failed. Please try again.");

    public static readonly Error LowScore = Error.Validation(
        "Recaptcha.LowScore",
        "reCAPTCHA score too low. Please try again.");

    public static readonly Error Unavailable = Error.Validation(
        "Recaptcha.Unavailable",
        "reCAPTCHA verification is temporarily unavailable. Please try again later.");
}
