using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Tharga.Fortnox.Tests;

public class FortnoxConnectionServiceTests
{
    private const string TestClientId = "test-client-id";
    private const string TestClientSecret = "test-client-secret";
    private const string TestRedirectUri = "https://example.com/callback";

    private static FortnoxConnectionService CreateService(IHttpClientFactory? httpClientFactory = null)
    {
        var configuration = Substitute.For<IConfiguration>();
        httpClientFactory ??= Substitute.For<IHttpClientFactory>();

        var options = new Options
        {
            ClientId = TestClientId,
            ClientSecret = TestClientSecret,
            RedirectUri = new Uri(TestRedirectUri)
        };

        return new FortnoxConnectionService(configuration, httpClientFactory, options);
    }

    private static IHttpClientFactory CreateMockHttpClientFactory(HttpResponseMessage response)
    {
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri(Constants.Root) };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Constants.ClientName).Returns(client);
        return factory;
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
            Assert.Contains($"redirect_uri={TestRedirectUri}", uri.ToString());
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
    }
}

internal class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(response);
    }
}
