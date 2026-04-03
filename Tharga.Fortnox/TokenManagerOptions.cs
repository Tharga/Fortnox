namespace Tharga.Fortnox;

/// <summary>
/// Configuration options for <see cref="IFortnoxTokenManager"/>.
/// </summary>
public record TokenManagerOptions
{
    /// <summary>
    /// Number of seconds before actual token expiry to trigger a proactive refresh.
    /// Default is 60 seconds.
    /// </summary>
    public int RefreshBufferSeconds { get; init; } = 60;
}
