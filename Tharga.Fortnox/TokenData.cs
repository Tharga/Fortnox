namespace Tharga.Fortnox;

/// <summary>
/// Token Data
/// </summary>
public record TokenData
{
    /// <summary>
    /// The time when the token was created.
    /// </summary>
    public DateTime CreateTime { get; init; }

    /// <summary>
    /// The actual access token to be used for regular Fortnox calls.
    /// </summary>
    public string AccessToken { get; init; }

    /// <summary>
    /// The time when the token will expore.
    /// </summary>
    public DateTime ExpireTime { get; init; }

    /// <summary>
    /// The refresh token to be used to retrieve a new access token. This token can be used only once.
    /// </summary>
    public string RefreshToken { get; init; }
}