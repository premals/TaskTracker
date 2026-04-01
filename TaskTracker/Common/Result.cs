namespace TaskTracker.Common;

public class Result
{
    protected Result(bool succeeded, string? message = null, Error? error = null)
    {
        if (succeeded && error is not null)
        {
            throw new InvalidOperationException("A successful result cannot contain an error.");
        }

        if (!succeeded && error is null)
        {
            throw new InvalidOperationException("A failed result must contain an error.");
        }

        Succeeded = succeeded;
        Message = message;
        Error = error;
    }

    public bool Succeeded { get; }

    public string? Message { get; }

    public Error? Error { get; }

    public static Result Success(string? message = null) => new(true, message);

    public static Result Failure(Error error) => new(false, error.Message, error);
}

public sealed class Result<T> : Result
{
    private Result(bool succeeded, T? data, string? message = null, Error? error = null)
        : base(succeeded, message, error)
    {
        Data = data;
    }

    public T? Data { get; }

    public static Result<T> Success(T data, string? message = null) => new(true, data, message);

    public static new Result<T> Failure(Error error) => new(false, default, error.Message, error);
}
