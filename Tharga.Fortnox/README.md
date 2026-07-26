# Tharga.Fortnox

OAuth token management for the [Fortnox](https://www.fortnox.se) API. Built for .NET 8 / 9 / 10. Handles the connection flow (`BuildConnectUriAsync` → `ConnectAsync` → `DisconnectAsync`), persistent access-token caching with thread-safe proactive refresh, and a small `Result<T>` surface for explicit success/failure handling.

This package handles **authentication only** — pair it with any Fortnox REST client of your choice.

## Install

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

## Documentation

Full guides, the connection-flow walkthrough, scopes reference, and API docs: [fortnox.tharga.net](https://fortnox.tharga.net).

[![GitHub repo](https://img.shields.io/github/repo-size/Tharga/Fortnox?style=flat&logo=github&logoColor=red&label=Repo)](https://github.com/Tharga/Fortnox)
