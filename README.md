# Tharga Fortnox
[![NuGet](https://img.shields.io/nuget/v/Tharga.Fortnox)](https://www.nuget.org/packages/Tharga.Fortnox)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Fortnox)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![GitHub repo Issues](https://img.shields.io/github/issues/Tharga/Fortnox?style=flat&logo=github&logoColor=red&label=Issues)](https://github.com/Tharga/Fortnox/issues?q=is%3Aopen)

## Get started

### Register in IOC.
This makes it possible to inject *IFortnoxConnectionService* when you need to handle access tokens to FortNox.
```
var builder = WebApplication.CreateBuilder(args);

...

builder.Services.AddThargaFortnox();

var app = builder.Build();

app.Run();
```

### Configuration
Configure by code.
```
builder.Services.AddThargaFortnox(o =>
{
    o.ClientSecret = "<Your Client Secret here>";
    o.ClientId = "<Your Client Id here>";
    o.RedirectUri = new Uri("<Your redirect Url here>");
});
```
Configure by *appsettings.json*.
```
{
  "Fortnox": {
    "ClientSecret": "<Your Client Secret here>",
    "ClientId": "<Your Client Id here>"
    "RedirectUri": "<Your redirect Url here>",
  }
}
```

## Usage
This component is intended for a backend service that communicats with front-end.
To start using it the user first must grant access to the Fortnox instance.
This is done with a specific call to Fortnox. A url can be generated using the method *BuildConnectUriAsync* in *IFortnoxConnectionService*.

This URL can be used as a link or redirect from your frontend. It will take the user to Fortnox to signin and allow permissions for your application for selected scopes.
On completion the user will be redirected back to *RedirectUri* set in *Options*.

```
[HttpGet]
[Route("link")]
public async Task<IActionResult> BuildConnectLinkUri([FromHeader] string tenantId)
{
    if (string.IsNullOrEmpty(tenantId)) return BadRequest($"No {nameof(tenantId)} provided");

    var requestKey = Guid.NewGuid();

    //TODO: Store current tenant with the requestKey, so you can connect them when the callback comes from Fortnox.

    //TODO: Select what scopes your application requires.
    var scopes = FortnoxScope.CompanyInformation | FortnoxScope.Bookkeeping;

    var uri = await _fortnoxConnectionService.BuildConnectUriAsync(requestKey.ToString(), scopes);

    return Ok(uri);
}
```

One the callback is made (to your front end-end) your can extract the information from Fortnox. Here is an example for Blazor WASM.
Note that this page have the route of HTTP-GET '/fortnox/connect' and that the code calls a HTTP-POST '/fortnox/connect' to backend.

The callback query contains *code* and *state*. That can be used to connect your application with Fortnox.
If there is an error the callback have the query parameter *error* set.
```
@page "/fortnox/connect"
@using Tharga.Fortnox
@inject HttpClient HttpClient
@inject NavigationManager NavigationManager

@if (!string.IsNullOrEmpty(_errorMessage))
{
    <p>Error: @_errorMessage</p>
}
else
{
    <p>@_message</p>
}

@code {
    private string _errorMessage;
    private string _message;

    protected override async Task OnInitializedAsync()
    {
        var uriBuilder = new UriBuilder(NavigationManager.Uri);
        var q = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

        if (q["error"] != null)
        {
            _errorMessage = $"Error: {q["error"]} ({q["error_description"]}, s:{q["state"]})";
            return;
        }

        if (!Guid.TryParse(q["state"], out var requestKey))
        {
            _errorMessage = $"Cannot use state {q["state"]}";
            return;
        }

        var assignment = new FortnoxAssignment { Code = q["code"], RequestKey = requestKey };
        using var result = await HttpClient.PostAsJsonAsync("fortnox/connect", assignment);
        if (!result.IsSuccessStatusCode)
        {
            _errorMessage = result.ReasonPhrase;
        }
        else
        {
            _message = "Your company has been linked with Fortnox.";
        }

        await base.OnInitializedAsync();
    }
}
```

The code above will make a POST call to the backend at route 'fortnox/connect' this is where you can perform the actual connection with Fortnox using method *ConnectAsync* in *IFortnoxConnectionService*.

```
[HttpPost]
[AllowAnonymous]
[Route("connect")]
public async Task<IActionResult> Connect(FortnoxAssignment assignment)
{
    if (assignment == null) return BadRequest($"No {nameof(assignment)} provided.");

    var tenantId = "<tennantId>"; //TODO: Get the tenantId using 'assignment.RequestKey' that was stored in 'BuildConnectLinkUri'.
    if (tenantId == null) throw new InvalidOperationException($"Cannot find tenant for request key '{assignment.RequestKey}'.");

    //NOTE: Perform the actual connection with Fortnox.
    var tokenDataResult = await _fortnoxConnectionService.ConnectAsync(assignment);
    if (!tokenDataResult.IsSuccess)
    {
        return BadRequest(tokenDataResult.Message);
    }

    //TODO: Store the token with the tenant.

    //NOTE: Request company information (optional code)
    var authorization = new StandardAuth(tokenDataResult.Value.AccessToken);
    var fortnoxClient = new FortnoxClient(authorization);
    var companyInformation = await fortnoxClient.CompanyInformationConnector.GetAsync();

    return Ok(companyInformation.CompanyName);
}
```

When theese steps are complete, your backend code can perform calls to Fortnox using the token.
It is valid for one hour and when the call fails you can use *RefreshTokenAsync* in *IFortnoxConnectionService* to renew the token with *refreshToken* as parameter.
The *refreshToken* can only be used once, if it fails the connection will have to be performed again.

To disconnect the application simply use *DisconnectAsync* in *IFortnoxConnectionService*.
This only requires the *refreshToken*.

### Full example of the Backend controller
In this example the *tenantId* is used as a central concept. There are no examples for autnentication and assurance that the correct tehhent have access, this code you have to add to have a safe solution.
```
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FortnoxController : ControllerBase
{
    private readonly IFortnoxConnectionService _fortnoxConnectionService;

    public FortnoxController(IFortnoxConnectionService fortnoxConnectionService)
    {
        _fortnoxConnectionService = fortnoxConnectionService;
    }

    [HttpGet]
    [Route("link")]
    public async Task<IActionResult> BuildConnectLinkUri([FromHeader] string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId)) return BadRequest($"No {nameof(tenantId)} provided");

        var requestKey = Guid.NewGuid();

        //TODO: Store current tenant with the requestKey, so you can connect them when the callback comes from Fortnox.

        //TODO: Select what scopes your application requires.
        var scopes = FortnoxScope.CompanyInformation | FortnoxScope.Bookkeeping;

        var uri = await _fortnoxConnectionService.BuildConnectUriAsync(requestKey.ToString(), scopes);

        return Ok(uri);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("connect")]
    public async Task<IActionResult> Connect(FortnoxAssignment assignment)
    {
        if (assignment == null) return BadRequest($"No {nameof(assignment)} provided.");

        var tenantId = "<tennantId>"; //TODO: Get the tenantId using 'assignment.RequestKey' that was stored in 'BuildConnectLinkUri'.
        if (tenantId == null) throw new InvalidOperationException($"Cannot find tenant for request key '{assignment.RequestKey}'.");

        //NOTE: Perform the actual connection with Fortnox.
        var tokenDataResult = await _fortnoxConnectionService.ConnectAsync(assignment);
        if (!tokenDataResult.IsSuccess)
        {
            return BadRequest(tokenDataResult.Message);
        }

        //TODO: Store the token with the tenant.

        //NOTE: Request company information (optional code)
        var authorization = new StandardAuth(tokenDataResult.Value.AccessToken);
        var fortnoxClient = new FortnoxClient(authorization);
        var companyInformation = await fortnoxClient.CompanyInformationConnector.GetAsync();

        return Ok(companyInformation.CompanyName);
    }

    [HttpPost]
    [Route("disconnect")]
    public async Task<IActionResult> Disconnect([FromHeader] string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId)) return BadRequest($"No {nameof(tenantId)} provided");

        var refreshToken = "<refreshTokenId>"; //TODO: Get the refreshToken using tenantId.

        await _fortnoxConnectionService.DisconnectAsync(refreshToken);

        return Ok();
    }
}
```