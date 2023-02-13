using System.Text.Json.Serialization;

namespace Flagrum.Services.Vendor.Patreon;

public class PatreonTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; }

    [JsonPropertyName("expires_in")] public int SecondsUntilExpiry { get; set; }

    [JsonPropertyName("scope")] public string Scope { get; set; }

    [JsonPropertyName("token_type")] public string TokenType { get; set; }
}