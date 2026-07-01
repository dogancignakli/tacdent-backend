namespace Tacdent.Application.Options;

public class RecaptchaOptions
{
    public const string SectionName = "Recaptcha";

    public string SecretKey { get; set; } = string.Empty;

    public double MinScore { get; set; } = 0.5;

    public bool Enabled { get; set; } = true;

    public string VerifyEndpoint { get; set; } = "https://www.google.com/recaptcha/api/siteverify";
}
