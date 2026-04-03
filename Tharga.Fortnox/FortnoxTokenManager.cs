using System.Collections.Concurrent;

namespace Tharga.Fortnox;

internal class FortnoxTokenManager : IFortnoxTokenManager
{
    private readonly IFortnoxConnectionService _connectionService;
    private readonly TokenManagerOptions _options;
    private readonly ConcurrentDictionary<string, TokenEntry> _entries = new();

    internal FortnoxTokenManager(IFortnoxConnectionService connectionService, TokenManagerOptions options)
    {
        _connectionService = connectionService;
        _options = options;
    }

    public async Task<Result<string>> GetAccessTokenAsync(string key, Func<Task<TokenData>> loadToken, Func<TokenData, Task> saveToken)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(loadToken);
        ArgumentNullException.ThrowIfNull(saveToken);

        var entry = _entries.GetOrAdd(key, _ => new TokenEntry());

        if (TryGetValidToken(entry, out var accessToken))
            return Result<string>.Success(accessToken);

        await entry.Lock.WaitAsync();
        try
        {
            if (TryGetValidToken(entry, out accessToken))
                return Result<string>.Success(accessToken);

            var tokenData = await loadToken();
            if (tokenData == null)
                return Result<string>.Fail("No token data available.", "NoToken");

            if (IsTokenValid(tokenData))
            {
                entry.TokenData = tokenData;
                return Result<string>.Success(tokenData.AccessToken);
            }

            var refreshResult = await _connectionService.RefreshTokenAsync(tokenData.RefreshToken);
            if (!refreshResult.IsSuccess)
                return Result<string>.Fail(refreshResult.Message, refreshResult.Code);

            await saveToken(refreshResult.Value);
            entry.TokenData = refreshResult.Value;
            return Result<string>.Success(refreshResult.Value.AccessToken);
        }
        finally
        {
            entry.Lock.Release();
        }
    }

    private bool TryGetValidToken(TokenEntry entry, out string accessToken)
    {
        var tokenData = entry.TokenData;
        if (tokenData != null && IsTokenValid(tokenData))
        {
            accessToken = tokenData.AccessToken;
            return true;
        }

        accessToken = null;
        return false;
    }

    private bool IsTokenValid(TokenData tokenData)
    {
        return tokenData.ExpireTime > DateTime.UtcNow.AddSeconds(_options.RefreshBufferSeconds);
    }

    private sealed class TokenEntry
    {
        public SemaphoreSlim Lock { get; } = new(1, 1);
        public TokenData TokenData { get; set; }
    }
}
