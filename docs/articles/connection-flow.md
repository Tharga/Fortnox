# Connection flow

The OAuth flow has three steps: build a connect URI, exchange the authorization code from Fortnox's callback for a token, and (when the user disconnects) revoke the refresh token.

## 1. Build the connect URI

Generate a per-request key — your application uses this to correlate the eventual callback back to the right tenant — and pass it together with the scopes your app needs:

```csharp
[HttpGet("link")]
public async Task<IActionResult> BuildConnectLink([FromHeader] string tenantId)
{
    if (string.IsNullOrEmpty(tenantId)) return BadRequest("No tenantId provided.");

    var requestKey = Guid.NewGuid();

    // Store (requestKey -> tenantId) so the callback can find the tenant.
    await _store.SaveRequestAsync(requestKey, tenantId);

    var scopes = FortnoxScope.CompanyInformation | FortnoxScope.Bookkeeping;
    var uri = await _fortnoxConnectionService.BuildConnectUriAsync(requestKey.ToString(), scopes);
    return Ok(uri);
}
```

The returned URI takes the user to Fortnox, where they sign in and approve the scopes. On completion Fortnox redirects them back to the `RedirectUri` from `Options`, with `code` and `state` query parameters (or `error` and `error_description` if it failed).

## 2. Handle the callback

The redirect target is yours to design. A Blazor page that posts the assignment to your backend:

```razor
@page "/fortnox/connect"
@using Tharga.Fortnox
@inject HttpClient HttpClient
@inject NavigationManager Nav

@code {
    protected override async Task OnInitializedAsync()
    {
        var q = System.Web.HttpUtility.ParseQueryString(new Uri(Nav.Uri).Query);
        if (q["error"] != null) { /* surface error */ return; }
        if (!Guid.TryParse(q["state"], out var requestKey)) return;

        var assignment = new FortnoxAssignment
        {
            Code = q["code"],
            RequestKey = requestKey,
        };
        await HttpClient.PostAsJsonAsync("fortnox/connect", assignment);
    }
}
```

## 3. Exchange the code for a token

```csharp
[HttpPost("connect")]
public async Task<IActionResult> Connect(FortnoxAssignment assignment)
{
    var tenantId = await _store.GetTenantAsync(assignment.RequestKey);
    if (tenantId == null) return BadRequest("Unknown request key.");

    var result = await _fortnoxConnectionService.ConnectAsync(assignment);
    if (!result.IsSuccess) return BadRequest(result.Message);

    // Persist the TokenData (AccessToken, RefreshToken, ExpireTime) for the tenant.
    await _store.SaveTokenAsync(tenantId, result.Value);
    return Ok();
}
```

`TokenData` has `AccessToken` (valid ~1 hour), `RefreshToken` (single-use), `CreateTime`, and `ExpireTime`. Persist all four — the [token manager](token-manager.md) reads them back on subsequent calls.

## 4. Disconnect

```csharp
[HttpPost("disconnect")]
public async Task<IActionResult> Disconnect([FromHeader] string tenantId)
{
    var refreshToken = await _store.GetRefreshTokenAsync(tenantId);
    var result = await _fortnoxConnectionService.DisconnectAsync(refreshToken);
    if (!result.IsSuccess) return BadRequest(result.Message);

    await _store.DeleteTokenAsync(tenantId);
    return Ok();
}
```

`DisconnectAsync` revokes the refresh token at Fortnox. After this, the user has to grant access again.

## Why `RequestKey`?

The state parameter is a Fortnox-required CSRF guard: Fortnox returns whatever you sent back to your callback unchanged. Use a fresh `Guid` per attempt, store it server-side with the tenant context, and only accept the callback if the state matches an outstanding request.
