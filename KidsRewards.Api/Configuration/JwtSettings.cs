namespace KidsRewards.Api.Configuration;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "KidsRewards.Api";
    public string Audience { get; set; } = "KidsRewards.Frontend";
    public int ExpirationMinutes { get; set; } = 480;
}
