namespace Tharga.Fortnox;

/// <summary>
/// Configuration options.
/// </summary>
public record Options
{
    /// <summary>
    /// The uri that will be used for callbacks from Fortnox when a connection has been made.
    /// </summary>
    public Uri RedirectUri { get; set; }

    /// <summary>
    /// The id of you application.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Fortnox Client Secret.
    /// </summary>
    public string ClientSecret { get; set; }
}