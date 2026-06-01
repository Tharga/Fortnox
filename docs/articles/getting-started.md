# Getting started

## Install

```
dotnet add package Tharga.Fortnox
```

## Register

```csharp
builder.Services.AddThargaFortnox();
```

## Configure

By code:

```csharp
builder.Services.AddThargaFortnox(o =>
{
    o.ClientId = "<Your Fortnox Client Id>";
    o.ClientSecret = "<Your Fortnox Client Secret>";
    o.RedirectUri = new Uri("https://your-app/fortnox/connect");
});
```

Or via `appsettings.json`:

```json
{
  "Fortnox": {
    "ClientId": "<Your Fortnox Client Id>",
    "ClientSecret": "<Your Fortnox Client Secret>",
    "RedirectUri": "https://your-app/fortnox/connect"
  }
}
```

Configuration is resolved lazily — values are read on first use, so missing keys surface as an `InvalidOperationException` only when you actually try to build a connect URI or refresh a token.

## What gets registered

`AddThargaFortnox` registers two services:

| Service | Lifetime | Purpose |
|---|---|---|
| `IFortnoxConnectionService` | Transient | Builds connect URIs, exchanges authorization codes for tokens, refreshes/revokes tokens. See [Connection flow](connection-flow.md). |
| `IFortnoxTokenManager` | Singleton | Caches and proactively refreshes access tokens per key. See [Token manager](token-manager.md). |

Both share the same `Options` and `HttpClient` factory entry, so configuration applies uniformly.

## Next

- [Connection flow](connection-flow.md) — get a user-authorized token for the first time
- [Token manager](token-manager.md) — keep using that token across calls without manually checking expiry
- [Scopes](scopes.md) — pick the right `FortnoxScope` flags for what your app needs
