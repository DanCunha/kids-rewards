namespace KidsRewards.Api.Configuration;

public class PasswordResetSettings
{
    public int TokenExpirationMinutes { get; set; } = 30;
    public string FrontendBaseUrl { get; set; } = "http://localhost:3000";
}
