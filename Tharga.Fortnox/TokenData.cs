namespace Tharga.Fortnox;

public record TokenData
{
    public DateTime CreateTime { get; init; }
    public string AccessToken { get; init; }
    public DateTime ExpireTime { get; init; }
    public string RefreshToken { get; init; }
}