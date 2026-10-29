using System.Net;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Tharga.Fortnox.Tests;

public class FortnoxConnectionServiceTests
{
    private const string TestClientId = "test-client-id";
    private const string TestClientSecret = "test-client-secret";
    private const string TestRedirectUri = "https://example.com/callback";
    private const string RedirectUriWithQuery = "https://app.example/callback?tenant=1&env=prod";
    private const string EncodedRedirectUriWithQuery = "https%3A%2F%2Fapp.example%2Fcallback%3Ftenant%3D1%26env%3Dprod";
    private const string ValueWithReservedCharacters = "a+b&c=d";
    private const string EncodedValueWithReservedCharacters = "a%2Bb%26c%3Dd";

    private static FortnoxConnectionService CreateService(IHttpClientFactory? httpClientFactory = null, string redirectUri = TestRedirectUri)
    {
        var configuration = Substitute.For<IConfiguration>();
        httpClientFactory ??= Substitute.For<IHttpClientFactory>();

        var options = new Options
        {
            ClientId = TestClientId,
            ClientSecret = TestClientSecret,
            RedirectUri = new Uri(redirectUri)
        };

        return new FortnoxConnectionService(configuration, httpClientFactory, options);
    }

    private static IHttpClientFactory CreateMockHttpClientFactory(HttpResponseMessage response)
    {
        return CreateMockHttpClientFactory(new FakeHttpMessageHandler(response));
    }

    private static IHttpClientFactory CreateMockHttpClientFactory(FakeHttpMessageHandler handler)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri(Constants.Root) };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Constants.ClientName).Returns(client);
        return factory;
    }

    private static HttpResponseMessage CreateTokenResponse()
    {
        var tokenResponse = new
        {
            access_token = "access",
            refresh_token = "refresh",
            expires_in = 3600,
            token_type = "bearer",
            scope = "customer"
        };
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
        };
    }

    public class BuildConnectUriAsync
    {
        [Fact]
        public async Task Throws_When_RequestKey_Is_Null()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.BuildConnectUriAsync(null, FortnoxScope.Customer).AsTask());
        }

        [Fact]
        public async Task Throws_When_RequestKey_Is_Empty()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.BuildConnectUriAsync("", FortnoxScope.Customer).AsTask());
        }

        [Fact]
        public async Task Throws_When_Scopes_Is_Zero()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.BuildConnectUriAsync("key", 0).AsTask());
        }

        [Fact]
        public async Task Returns_Uri_With_Single_Scope()
        {
            var service = CreateService();

            var uri = await service.BuildConnectUriAsync("my-key", FortnoxScope.Customer);

            Assert.Contains("scope=customer", uri.ToString());
            Assert.Contains("state=my-key", uri.ToString());
            Assert.Contains($"client_id={TestClientId}", uri.ToString());
            Assert.Contains("redirect_uri=https%3A%2F%2Fexample.com%2Fcallback", uri.AbsoluteUri);
        }

        [Fact]
        public async Task Returns_Uri_With_Multiple_Scopes()
        {
            var service = CreateService();

            var uri = await service.BuildConnectUriAsync("key", FortnoxScope.Customer | FortnoxScope.Invoice);

            var uriString = uri.ToString();
            Assert.Contains("customer", uriString);
            Assert.Contains("invoice", uriString);
        }

        [Fact]
        public async Task Scope_Names_Are_Lowercased()
        {
            var service = CreateService();

            var uri = await service.BuildConnectUriAsync("key", FortnoxScope.CompanyInformation);

            Assert.Contains("companyinformation", uri.ToString());
        }

        [Fact]
        public async Task Uri_Starts_With_Fortnox_Auth_Root()
        {
            var service = CreateService();

            var uri = await service.BuildConnectUriAsync("key", FortnoxScope.Customer);

            Assert.StartsWith($"{Constants.Root}auth", uri.ToString());
        }

        [Fact]
        public async Task Encodes_Redirect_Uri_With_Query_As_Single_Value()
        {
            var service = CreateService(redirectUri: RedirectUriWithQuery);

            var uri = await service.BuildConnectUriAsync("key", FortnoxScope.Customer);

            var query = HttpUtility.ParseQueryString(uri.Query);
            Assert.EndsWith($"&redirect_uri={EncodedRedirectUriWithQuery}", uri.AbsoluteUri);
            Assert.Equal(RedirectUriWithQuery, query["redirect_uri"]);
            Assert.Null(query["env"]);
        }
    }

    public class ConnectAsync
    {
        [Fact]
        public async Task Returns_Success_With_TokenData_On_200()
        {
            var tokenResponse = new
            {
                access_token = "access-123",
                refresh_token = "refresh-456",
                expires_in = 3600,
                token_type = "bearer",
                scope = "customer"
            };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
            };
            var factory = CreateMockHttpClientFactory(response);
            var service = CreateService(factory);

            var result = await service.ConnectAsync(new FortnoxAssignment { Code = "auth-code" });

            Assert.True(result.IsSuccess);
            Assert.Equal("access-123", result.Value.AccessToken);
            Assert.Equal("refresh-456", result.Value.RefreshToken);
        }

        [Fact]
        public async Task Returns_Fail_On_Error_Response()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "Bad Request"
            };
            var factory = CreateMockHttpClientFactory(response);
            var service = CreateService(factory);

            var result = await service.ConnectAsync(new FortnoxAssignment { Code = "bad-code" });

            Assert.False(result.IsSuccess);
            Assert.Equal("BadRequest", result.Code);
        }

        [Fact]
        public async Task Returns_Fail_On_Error_Response_Without_Reason_Phrase()
        {
            var response = new HttpResponseMessage((HttpStatusCode)520);
            var factory = CreateMockHttpClientFactory(response);
            var service = CreateService(factory);

            var result = await service.ConnectAsync(new FortnoxAssignment { Code = "auth-code" });

            Assert.False(result.IsSuccess);
            Assert.Equal("520", result.Code);
        }

        [Fact]
        public async Task Sends_Code_And_Redirect_Uri_Form_Encoded()
        {
            var handler = new FakeHttpMessageHandler(CreateTokenResponse());
            var service = CreateService(CreateMockHttpClientFactory(handler), RedirectUriWithQuery);

            await service.ConnectAsync(new FortnoxAssignment { Code = ValueWithReservedCharacters });

            Assert.Equal($"grant_type=authorization_code&code={EncodedValueWithReservedCharacters}&redirect_uri={EncodedRedirectUriWithQuery}", handler.RequestBody);
        }
    }

    public class RefreshTokenAsync
    {
        [Fact]
        public async Task Returns_Success_With_TokenData_On_200()
        {
            var tokenResponse = new
            {
                access_token = "new-access",
                refresh_token = "new-refresh",
                expires_in = 3600,
                token_type = "bearer",
                scope = "customer"
            };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
            };
            var factory = CreateMockHttpClientFactory(response);
            var service = CreateService(factory);

            var result = await service.RefreshTokenAsync("old-refresh-token");

            Assert.True(result.IsSuccess);
            Assert.Equal("new-access", result.Value.AccessToken);
            Assert.Equal("new-refresh", result.Value.RefreshToken);
        }

        [Fact]
        public async Task Returns_Fail_On_Error_Response()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Unauthorized"
            };
            var factory = CreateMockHttpClientFactory(response);
            var service = CreateService(factory);

            var result = await service.RefreshTokenAsync("invalid-token");

            Assert.False(result.IsSuccess);
            Assert.Equal("Unauthorized", result.Code);
        }

        [Fact]
        public async Task Returns_Fail_On_Error_Response_Without_Reason_Phrase()
        {
            var response = new HttpResponseMessage((HttpStatusCode)520);
            var factory = CreateMockHttpClientFactory(response);
            var service = CreateService(factory);

            var result = await service.RefreshTokenAsync("refresh-token");

            Assert.False(result.IsSuccess);
            Assert.Equal("520", result.Code);
        }

        [Fact]
        public async Task Sends_Refresh_Token_Form_Encoded()
        {
            var handler = new FakeHttpMessageHandler(CreateTokenResponse());
            var service = CreateService(CreateMockHttpClientFactory(handler));

            await service.RefreshTokenAsync(ValueWithReservedCharacters);

            Assert.Equal($"grant_type=refresh_token&refresh_token={EncodedValueWithReservedCharacters}", handler.RequestBody);
        }
    }

    public class DisconnectAsync
    {
        [Fact]
        public async Task Returns_Success_On_200()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var factory = CreateMockHttpClientFactory(response);
            var service = CreateService(factory);

            var result = await service.DisconnectAsync("refresh-token");

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Returns_Fail_On_Error_Response()
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "Internal Server Error"
            };
            var factory = CreateMockHttpClientFactory(response);
            var service = CreateService(factory);

            var result = await service.DisconnectAsync("refresh-token");

            Assert.False(result.IsSuccess);
            Assert.Equal("InternalServerError", result.Code);
        }

        [Fact]
        public async Task Returns_Fail_On_Error_Response_Without_Reason_Phrase()
        {
            var response = new HttpResponseMessage((HttpStatusCode)520);
            var factory = CreateMockHttpClientFactory(response);
            var service = CreateService(factory);

            var result = await service.DisconnectAsync("refresh-token");

            Assert.False(result.IsSuccess);
            Assert.Equal("520", result.Code);
        }

        [Fact]
        public async Task Sends_Token_Form_Encoded()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
            var service = CreateService(CreateMockHttpClientFactory(handler));

            await service.DisconnectAsync(ValueWithReservedCharacters);

            Assert.Equal($"token_type_hint=refresh_token&token={EncodedValueWithReservedCharacters}", handler.RequestBody);
        }
    }
}

internal class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    public string? RequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestBody = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        return response;
    }
}
