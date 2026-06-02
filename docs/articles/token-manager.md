# Token manager

Fortnox access tokens live about an hour, and the refresh token is single-use — once you swap it for a new pair, the old one is dead. That makes concurrent refresh races a real problem: two threads both notice the token is stale, both try to refresh, one of them gets a 401 because the refresh token was already consumed.

`IFortnoxTokenManager` solves this with **per-key in-memory caching + per-key locking + proactive refresh**.

## Usage

```csharp
public class MyFortnoxService(IFortnoxTokenManager tokenManager)
{
    public async Task DoWorkAsync(string tenantId)
    {
        var result = await tokenManager.GetAccessTokenAsync(
            tenantId,
            loadToken: () => LoadTokenFromDb(tenantId),
            saveToken: token => SaveTokenToDb(tenantId, token));

        if (!result.IsSuccess)
        {
            // The token may need re-authorization (refresh chain broken).
            return;
        }

        var accessToken = result.Value;
        // ... use accessToken for Fortnox REST calls
    }
}
```

## How it works

1. **Cache lookup.** If the key has a cached token whose `ExpireTime` is more than `RefreshBufferSeconds` away, return it immediately. No I/O.
2. **Acquire the per-key lock.** Other threads asking for the same key wait here. Different keys do not contend.
3. **Recheck after acquiring the lock.** Another thread may have refreshed while we waited.
4. **Load from storage.** Call your `loadToken` delegate.
5. **Validate.** If the loaded token is still fresh, cache it and return.
6. **Refresh.** If stale, call `IFortnoxConnectionService.RefreshTokenAsync` with the loaded refresh token.
7. **Persist + cache.** Call your `saveToken` delegate with the new `TokenData`, store it in the cache, return the access token.

The lock is released regardless of outcome. Errors propagate as `Result.Fail(...)`.

## Configuration

```csharp
builder.Services.AddThargaFortnox(o =>
{
    o.ClientId = "...";
    o.ClientSecret = "...";
    o.RedirectUri = new Uri("...");
    o.TokenManager = new TokenManagerOptions
    {
        RefreshBufferSeconds = 120 // refresh 2 minutes before expiry
    };
});
```

Default `RefreshBufferSeconds = 60`. Increase it if your network round-trips are slow or if you want larger safety margin for long-running operations that started just before expiry.

## Multiple tenants

Each `key` is an independent connection. Pass whatever identifier ties the token to its owner — typically a company id, tenant id, or `CustomerNumber`. The manager isolates locks and cache entries by key, so one tenant's refresh never blocks another's.

```csharp
await tokenManager.GetAccessTokenAsync("tenant-a", ...);  // independent
await tokenManager.GetAccessTokenAsync("tenant-b", ...);  // from each other
```

## Limitation: single-instance only

The cache is in-process. If you run multiple server instances against the same Fortnox connection, instance A's refresh invalidates the refresh token instance B has cached — the next call from B fails because Fortnox no longer recognises B's now-old refresh token.

For multi-instance deployments either:

- Run a single dedicated worker that owns the refresh, and have other instances request access tokens from it; or
- Coordinate refresh via a distributed lock + shared store (the `loadToken`/`saveToken` delegates already give you the seam — point them at a row in your shared database, and add a distributed lock around the refresh).

`Tharga.Fortnox` deliberately keeps this out of scope: the right shared store is project-specific (SQL, Redis, MongoDB, etc.), and pretending otherwise would mean shipping a dependency on whichever we picked.
