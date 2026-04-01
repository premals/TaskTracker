namespace TaskTracker.Common;

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyDictionary<string, string[]>? Details = null)
{
    public static Error Validation(
        string code,
        string message,
        IReadOnlyDictionary<string, string[]> details) =>
        new(code, message, ErrorType.Validation, details);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Unexpected(
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? details = null) =>
        new(code, message, ErrorType.Unexpected, details);
}
