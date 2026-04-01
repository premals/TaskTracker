using FluentValidation.Results;

namespace TaskTracker.Common;

public static class FluentValidationResultExtensions
{
    public static Result<T> ToFailureResult<T>(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            throw new InvalidOperationException("Cannot create a failure result from a valid FluentValidation result.");
        }

        var details = validationResult.Errors
            .GroupBy(
                failure => string.IsNullOrWhiteSpace(failure.PropertyName) ? "Request" : failure.PropertyName,
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray(),
                StringComparer.Ordinal);

        var distinctCodes = validationResult.Errors
            .Select(failure => failure.ErrorCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var errorCode = distinctCodes.Length == 1
            ? distinctCodes[0]
            : "validation_error";

        var message = errorCode switch
        {
            "task_done_requires_title" => "A task cannot be marked as Done when the title is empty or whitespace.",
            _ => "One or more validation errors occurred."
        };

        return Result<T>.Failure(Error.Validation(errorCode, message, details));
    }
}
