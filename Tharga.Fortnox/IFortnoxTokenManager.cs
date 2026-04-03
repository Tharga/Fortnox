namespace Tharga.Fortnox;

/// <summary>
/// Manages Fortnox access tokens with in-memory caching and thread-safe refresh.
/// Each key represents an independent Fortnox connection (e.g. per company or tenant).
/// <para>
/// <b>Single-instance only:</b> The in-memory cache is not shared across server instances.
/// If multiple servers use the same Fortnox token, one server's refresh may invalidate
/// another's cached token since refresh tokens are single-use.
/// </para>
/// </summary>
public interface IFortnoxTokenManager
{
    /// <summary>
    /// Returns a valid access token for the given key, refreshing automatically if expired.
    /// Uses in-memory caching with per-key locking to prevent concurrent refresh races.
    /// </summary>
    /// <param name="key">Unique identifier for the Fortnox connection (e.g. company ID).</param>
    /// <param name="loadToken">Delegate to load the current <see cref="TokenData"/> from persistent storage.</param>
    /// <param name="saveToken">Delegate to save the refreshed <see cref="TokenData"/> to persistent storage.</param>
    /// <returns>A <see cref="Result{T}"/> containing the access token string on success, or an error on failure.</returns>
    Task<Result<string>> GetAccessTokenAsync(string key, Func<Task<TokenData>> loadToken, Func<TokenData, Task> saveToken);
}
