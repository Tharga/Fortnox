namespace Tharga.Fortnox;

/// <summary>
/// Result objec with data payload.
/// </summary>
/// <typeparam name="TData"></typeparam>
public record Result<TData> : Result
{
    /// <summary>
    /// Payload data
    /// </summary>
    public TData Value { get; private init; }

    /// <summary>
    /// Build a success response for your data.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public new static Result<TData> Success(TData data) => new() { Value = data };

    /// <summary>
    /// Build a fail response for your data.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public new static Result<TData> Fail(string message) => new() { Message = message };
}

/// <summary>
/// Result object without payload.
/// </summary>
public record Result
{
    /// <summary>
    /// Response message.
    /// </summary>
    public string Message { get; protected init; }

    /// <summary>
    /// True if the call was successful
    /// </summary>
    public bool IsSuccess => string.IsNullOrEmpty(Message);

    /// <summary>
    /// Build a success response without data.
    /// </summary>
    public static Result Success => new();

    /// <summary>
    /// Build a fail response without data.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Result Fail(string message) => new() { Message = message };
}