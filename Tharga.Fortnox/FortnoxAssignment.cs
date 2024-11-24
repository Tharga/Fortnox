namespace Tharga.Fortnox;

public record FortnoxAssignment
{
    public string Code { get; init; }
    public Guid RequestKey { get; init; }
}