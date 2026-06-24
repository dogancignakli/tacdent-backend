namespace Tacdent.Application.Options;

public class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>Bootstrap-only: used once to seed the initial admin if the users table is empty.</summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>Bootstrap-only: used once to seed the initial admin if the users table is empty.</summary>
    public string AdminPassword { get; set; } = string.Empty;

    public int MaxFailedAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;
}
