namespace Tharga.Fortnox;

public interface IFortnoxConnectionService
{
    ValueTask<Uri> BuildConnectUriAsync(string requestKey, FortnoxScope scopes);
    Task<Result<TokenData>> ConnectAsync(FortnoxAssignment assignment);
    Task<Result<TokenData>> RefreshTokenAsync(string refreshToken);
    Task<Result> DisconnectAsync(string refreshToken);
}