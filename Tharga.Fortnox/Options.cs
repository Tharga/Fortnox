namespace Tharga.Fortnox;

public record Options
{
    public Uri RedirectUri { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}