using NSubstitute;

namespace Tharga.Fortnox.Tests;

public class FortnoxTokenManagerTests
{
    private const int DefaultBufferSeconds = 60;

    private static TokenData CreateTokenData(int expiresInSeconds = 3600, string accessToken = "access-token", string refreshToken = "refresh-token")
    {
        return new TokenData
        {
            CreateTime = DateTime.UtcNow,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpireTime = DateTime.UtcNow.AddSeconds(expiresInSeconds)
        };
    }

    private static (FortnoxTokenManager manager, IFortnoxConnectionService connectionService) CreateManager(int bufferSeconds = DefaultBufferSeconds)
    {
        var connectionService = Substitute.For<IFortnoxConnectionService>();
        var options = new TokenManagerOptions { RefreshBufferSeconds = bufferSeconds };
        var manager = new FortnoxTokenManager(connectionService, options);
        return (manager, connectionService);
    }

    [Fact]
    public async Task Returns_AccessToken_When_Token_Is_Valid()
    {
        var (manager, connectionService) = CreateManager();
        var tokenData = CreateTokenData();

        var result = await manager.GetAccessTokenAsync("key1", () => Task.FromResult(tokenData), _ => Task.CompletedTask);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value);
        await connectionService.DidNotReceive().RefreshTokenAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Refreshes_When_Token_Is_Expired()
    {
        var (manager, connectionService) = CreateManager();
        var expiredToken = CreateTokenData(expiresInSeconds: -10);
        var refreshedToken = CreateTokenData(accessToken: "new-access", refreshToken: "new-refresh");

        connectionService.RefreshTokenAsync("refresh-token").Returns(Result<TokenData>.Success(refreshedToken));

        var result = await manager.GetAccessTokenAsync("key1", () => Task.FromResult(expiredToken), _ => Task.CompletedTask);

        Assert.True(result.IsSuccess);
        Assert.Equal("new-access", result.Value);
        await connectionService.Received(1).RefreshTokenAsync("refresh-token");
    }

    [Fact]
    public async Task Refreshes_When_Token_Is_Within_Buffer()
    {
        var (manager, connectionService) = CreateManager(bufferSeconds: 120);
        var almostExpired = CreateTokenData(expiresInSeconds: 60);
        var refreshedToken = CreateTokenData(accessToken: "new-access", refreshToken: "new-refresh");

        connectionService.RefreshTokenAsync(Arg.Any<string>()).Returns(Result<TokenData>.Success(refreshedToken));

        var result = await manager.GetAccessTokenAsync("key1", () => Task.FromResult(almostExpired), _ => Task.CompletedTask);

        Assert.True(result.IsSuccess);
        Assert.Equal("new-access", result.Value);
    }

    [Fact]
    public async Task Does_Not_Refresh_When_Token_Outside_Buffer()
    {
        var (manager, connectionService) = CreateManager(bufferSeconds: 60);
        var validToken = CreateTokenData(expiresInSeconds: 120);

        var result = await manager.GetAccessTokenAsync("key1", () => Task.FromResult(validToken), _ => Task.CompletedTask);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value);
        await connectionService.DidNotReceive().RefreshTokenAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Returns_Fail_When_Refresh_Fails()
    {
        var (manager, connectionService) = CreateManager();
        var expiredToken = CreateTokenData(expiresInSeconds: -10);

        connectionService.RefreshTokenAsync(Arg.Any<string>()).Returns(Result<TokenData>.Fail("Unauthorized", "401"));

        var result = await manager.GetAccessTokenAsync("key1", () => Task.FromResult(expiredToken), _ => Task.CompletedTask);

        Assert.False(result.IsSuccess);
        Assert.Equal("Unauthorized", result.Message);
        Assert.Equal("401", result.Code);
    }

    [Fact]
    public async Task Returns_Fail_When_LoadToken_Returns_Null()
    {
        var (manager, _) = CreateManager();

        var result = await manager.GetAccessTokenAsync("key1", () => Task.FromResult<TokenData>(null!), _ => Task.CompletedTask);

        Assert.False(result.IsSuccess);
        Assert.Equal("NoToken", result.Code);
    }

    [Fact]
    public async Task Calls_SaveToken_On_Successful_Refresh()
    {
        var (manager, connectionService) = CreateManager();
        var expiredToken = CreateTokenData(expiresInSeconds: -10);
        var refreshedToken = CreateTokenData(accessToken: "new-access");
        TokenData? savedToken = null;

        connectionService.RefreshTokenAsync(Arg.Any<string>()).Returns(Result<TokenData>.Success(refreshedToken));

        await manager.GetAccessTokenAsync("key1", () => Task.FromResult(expiredToken), t => { savedToken = t; return Task.CompletedTask; });

        Assert.NotNull(savedToken);
        Assert.Equal("new-access", savedToken.AccessToken);
    }

    [Fact]
    public async Task Cache_Hit_Does_Not_Call_LoadToken()
    {
        var (manager, _) = CreateManager();
        var tokenData = CreateTokenData();
        var loadCount = 0;

        await manager.GetAccessTokenAsync("key1", () => { loadCount++; return Task.FromResult(tokenData); }, _ => Task.CompletedTask);
        await manager.GetAccessTokenAsync("key1", () => { loadCount++; return Task.FromResult(tokenData); }, _ => Task.CompletedTask);

        Assert.Equal(1, loadCount);
    }

    [Fact]
    public async Task Different_Keys_Are_Independent()
    {
        var (manager, connectionService) = CreateManager();
        var token1 = CreateTokenData(accessToken: "token-a");
        var token2 = CreateTokenData(accessToken: "token-b");

        var result1 = await manager.GetAccessTokenAsync("company-1", () => Task.FromResult(token1), _ => Task.CompletedTask);
        var result2 = await manager.GetAccessTokenAsync("company-2", () => Task.FromResult(token2), _ => Task.CompletedTask);

        Assert.Equal("token-a", result1.Value);
        Assert.Equal("token-b", result2.Value);
    }

    [Fact]
    public async Task Late_Thread_Uses_Refreshed_Token_From_Cache()
    {
        var (manager, connectionService) = CreateManager();
        var expiredToken = CreateTokenData(expiresInSeconds: -10);
        var refreshedToken = CreateTokenData(accessToken: "refreshed", refreshToken: "new-refresh");

        connectionService.RefreshTokenAsync("refresh-token").Returns(Result<TokenData>.Success(refreshedToken));

        var task1 = manager.GetAccessTokenAsync("key1", () => Task.FromResult(expiredToken), _ => Task.CompletedTask);
        var task2 = manager.GetAccessTokenAsync("key1", () => Task.FromResult(expiredToken), _ => Task.CompletedTask);

        var results = await Task.WhenAll(task1, task2);

        Assert.True(results[0].IsSuccess);
        Assert.True(results[1].IsSuccess);
        Assert.Equal("refreshed", results[0].Value);
        Assert.Equal("refreshed", results[1].Value);
        await connectionService.Received(1).RefreshTokenAsync("refresh-token");
    }

    [Fact]
    public async Task Throws_On_Null_Key()
    {
        var (manager, _) = CreateManager();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            manager.GetAccessTokenAsync(null, () => Task.FromResult(CreateTokenData()), _ => Task.CompletedTask));
    }

    [Fact]
    public async Task Throws_On_Null_LoadToken()
    {
        var (manager, _) = CreateManager();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            manager.GetAccessTokenAsync("key1", null, _ => Task.CompletedTask));
    }

    [Fact]
    public async Task Throws_On_Null_SaveToken()
    {
        var (manager, _) = CreateManager();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            manager.GetAccessTokenAsync("key1", () => Task.FromResult(CreateTokenData()), null));
    }
}
