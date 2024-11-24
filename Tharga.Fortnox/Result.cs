namespace Tharga.Fortnox;

public record Result<TData> : Result
{
    public TData Value { get; private init; }

    public new static Result<TData> Success(TData data) => new() { Value = data };
    public new static Result<TData> Fail(string message) => new() { Message = message };
}

public record Result
{
    public string Message { get; protected init; }
    public bool IsSuccess => string.IsNullOrEmpty(Message);

    public static Result Success => new();
    public static Result Fail(string message) => new() { Message = message };
}