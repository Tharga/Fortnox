using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Tharga.Fortnox;

internal class FortnoxConnectionService : IFortnoxConnectionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Lazy<string> _redirectUri;
    private readonly Lazy<string> _clientSecret;
    private readonly Lazy<string> _clientId;

    internal FortnoxConnectionService(IConfiguration configuration, IHttpClientFactory httpClientFactory, Options options)
    {
        _httpClientFactory = httpClientFactory;

        _redirectUri = new Lazy<string>(() =>
        {
            var redirectUri = options.RedirectUri?.OriginalString ?? configuration.GetSection($"Fortnox:{nameof(Options.RedirectUri)}").Value;
            if (string.IsNullOrEmpty(redirectUri)) throw new InvalidOperationException($"Cannot find configuration for {nameof(Options.RedirectUri)} in {nameof(IConfiguration)} as 'Fortnox:{nameof(Options.RedirectUri)}' or as an option provided with {nameof(ThargaFortnoxRegistration.AddThargaFortnox)}.");
            return redirectUri;
        });

        _clientSecret = new Lazy<string>(() =>
        {
            var clientSecret = options.ClientSecret ?? configuration.GetSection($"Fortnox:{nameof(Options.ClientSecret)}").Value;
            if (string.IsNullOrEmpty(clientSecret)) throw new InvalidOperationException($"Cannot find configuration for {nameof(Options.ClientSecret)} in {nameof(IConfiguration)} as 'Fortnox:{nameof(Options.ClientSecret)}' or as an option provided with {nameof(ThargaFortnoxRegistration.AddThargaFortnox)}.");
            return clientSecret;
        });

        _clientId = new Lazy<string>(() =>
        {
            var clientId = options.ClientId ?? configuration.GetSection($"Fortnox:{nameof(Options.ClientId)}").Value;
            if (string.IsNullOrEmpty(clientId)) throw new InvalidOperationException($"Cannot find configuration for {nameof(Options.ClientId)} in {nameof(IConfiguration)} as 'Fortnox:{nameof(Options.ClientId)}' or as an option provided with {nameof(ThargaFortnoxRegistration.AddThargaFortnox)}.");
            return clientId;
        });
    }

    private string RedirectUri => _redirectUri.Value;
    private string ClientSecret => _clientSecret.Value;
    private string ClientId => _clientId.Value;

    public ValueTask<Uri> BuildConnectUriAsync(string requestKey, FortnoxScope scopes)
    {
        if (string.IsNullOrEmpty(requestKey)) throw new ArgumentNullException(nameof(requestKey));
        if (scopes == 0) throw new ArgumentNullException(nameof(scopes));

        var scope = string.Join(" ", GetScopes(scopes));

        var uri = $"{Constants.Root}auth?client_id={Uri.EscapeDataString(ClientId)}&scope={Uri.EscapeDataString(scope)}&state={Uri.EscapeDataString(requestKey)}&response_type=code&account_type=service&redirect_uri={Uri.EscapeDataString(RedirectUri)}";
        return ValueTask.FromResult(new Uri(uri));
    }

    public async Task<Result<TokenData>> ConnectAsync(FortnoxAssignment assignment)
    {
        var client = GetHttpClient();

        client.DefaultRequestHeaders.Add("ClientId", ClientId);
        client.DefaultRequestHeaders.Add("ClientSecret", ClientSecret);
        var content = new FormUrlEncodedContent([
            new("grant_type", "authorization_code"),
            new("code", assignment.Code),
            new("redirect_uri", RedirectUri)
        ]);
        var result = await client.PostAsync("token", content);
        if (!result.IsSuccessStatusCode) return Result<TokenData>.Fail(result.ReasonPhrase, $"{result.StatusCode}");

        var data = await BuildTokenData(result);
        return Result<TokenData>.Success(data);
    }

    public async Task<Result<TokenData>> RefreshTokenAsync(string refreshToken)
    {
        var client = GetHttpClient();

        var content = new FormUrlEncodedContent([
            new("grant_type", "refresh_token"),
            new("refresh_token", refreshToken)
        ]);
        var result = await client.PostAsync("token", content);
        if (!result.IsSuccessStatusCode) return Result<TokenData>.Fail(result.ReasonPhrase, $"{result.StatusCode}");

        var data = await BuildTokenData(result);
        return Result<TokenData>.Success(data);
    }

    public async Task<Result> DisconnectAsync(string refreshToken)
    {
        var client = GetHttpClient();

        var content = new FormUrlEncodedContent([
            new("token_type_hint", "refresh_token"),
            new("token", refreshToken)
        ]);
        var result = await client.PostAsync("revoke", content);
        if (!result.IsSuccessStatusCode) return Result.Fail(result.ReasonPhrase, $"{result.StatusCode}");
        return Result.Success;
    }

    private HttpClient GetHttpClient()
    {
        var credentials = ToBase64Encode($"{ClientId}:{ClientSecret}");

        var client = _httpClientFactory.CreateClient(Constants.ClientName);

        client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
        return client;
    }

    private static async Task<TokenData> BuildTokenData(HttpResponseMessage result)
    {
        var resultContent = await result.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<AccessTokenResponse>(resultContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var data = new TokenData
        {
            CreateTime = DateTime.UtcNow,
            AccessToken = token.AccessToken,
            ExpireTime = DateTime.UtcNow.AddSeconds(token.ExpiresIn - 60),
            RefreshToken = token.RefreshToken,
        };
        return data;
    }

    private static string ToBase64Encode(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(bytes);
    }

    private static IEnumerable<string> GetScopes(FortnoxScope scopes)
    {
        foreach (var s in (FortnoxScope[])Enum.GetValues(typeof(FortnoxScope)))
        {
            if (scopes.HasFlag(s))
            {
                yield return s.ToString().ToLower();
            }
        }
    }
}
