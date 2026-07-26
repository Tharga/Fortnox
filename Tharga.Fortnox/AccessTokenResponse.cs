using System.Text.Json.Serialization;

namespace Tharga.Fortnox;

/// <summary>
/// The OAuth token response returned by Fortnox. Property names are mapped
/// explicitly because the wire format is snake_case; deserialization is
/// case-insensitive but does not ignore underscores, so the mapping is
/// load-bearing.
/// </summary>
internal record AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; }

    [JsonPropertyName("scope")]
    public string Scope { get; init; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }
}
