---
_layout: landing
---

# Tharga.Fortnox

OAuth token management for the [Fortnox](https://www.fortnox.se) API. Built for **.NET 8 / 9 / 10**. Provides the authentication flow (`BuildConnectUriAsync` → `ConnectAsync` → `DisconnectAsync`), per-key access-token caching, thread-safe refresh, and a small `Result<T>` surface for explicit success/failure handling.

## Package

| Package | What it does |
|---|---|
| [Tharga.Fortnox](https://www.nuget.org/packages/Tharga.Fortnox) | OAuth connection service + token manager. No Fortnox API client — pair with any Fortnox REST client of your choice. |

## Quick start

```
dotnet add package Tharga.Fortnox
```

```csharp
builder.Services.AddThargaFortnox(o =>
{
    o.ClientId = "...";
    o.ClientSecret = "...";
    o.RedirectUri = new Uri("https://your-app/fortnox/connect");
});
```

```csharp
public class MyFortnoxService(IFortnoxTokenManager tokenManager)
{
    public async Task DoWorkAsync(string tenantId)
    {
        var result = await tokenManager.GetAccessTokenAsync(
            tenantId,
            loadToken: () => LoadTokenFromDb(tenantId),
            saveToken: token => SaveTokenToDb(tenantId, token));

        if (!result.IsSuccess) return;
        var accessToken = result.Value;
        // ... call Fortnox with accessToken
    }
}
```

## Where next

- **[Articles](articles/index.md)** — feature guides: getting started, connection flow, token manager, scopes
- **[API reference](xref:Tharga.Fortnox)** — every public type, method, and option, generated from XML doc comments
- **[GitHub](https://github.com/Tharga/Fortnox)** — source, issues, releases
