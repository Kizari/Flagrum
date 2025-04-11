using System.Text.Json.Serialization;

namespace Flagrum.Application.Features.Menu.Data;

public class GiftTokenResponse
{
    [JsonPropertyName("status")] public int Status { get; set; }
}

public enum GiftTokenRegistrationResponse
{
    InvalidToken,
    AlreadyRegistered,
    Success
}

public enum GiftTokenValidationResponse
{
    Valid,
    Invalid
}