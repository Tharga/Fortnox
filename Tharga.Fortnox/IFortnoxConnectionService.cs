namespace Tharga.Fortnox;

/// <summary>
/// Handles connections with Fortnox.
/// </summary>
public interface IFortnoxConnectionService
{
    /// <summary>
    /// Use this method to get a url to initiat connection with fortnox.
    /// </summary>
    /// <param name="requestKey">A key for the request so that callbacks can be identified. Your application can generate this by using 'Guid.NewGuid()'.</param>
    /// <param name="scopes">Flag enum to specify the access scopes requested.</param>
    /// <returns></returns>
    ValueTask<Uri> BuildConnectUriAsync(string requestKey, FortnoxScope scopes);

    /// <summary>
    /// Connect the application with Fortnox.
    /// </summary>
    /// <param name="assignment"></param>
    /// <returns></returns>
    Task<Result<TokenData>> ConnectAsync(FortnoxAssignment assignment);

    /// <summary>
    /// Retrieve a new access token using the refresh token.
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    Task<Result<TokenData>> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Disconnect the application from Fortnox.
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    Task<Result> DisconnectAsync(string refreshToken);
}