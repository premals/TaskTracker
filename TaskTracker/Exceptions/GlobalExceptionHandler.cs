using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using TaskTracker.Common;
using TaskTracker.Requests;

namespace TaskTracker.Exceptions;

public sealed class GlobalExceptionHandler(
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (TryMapToValidationError(exception, out var validationError))
        {
            logger.LogWarning(exception, "Invalid request while processing {Path}", httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(Result.Failure(validationError), cancellationToken);
            return true;
        }

        logger.LogError(exception, "Unhandled exception while processing {Path}", httpContext.Request.Path);

        var error = environment.IsDevelopment()
            ? Error.Unexpected(
                "unexpected_error",
                exception.Message,
                new Dictionary<string, string[]>
                {
                    ["ExceptionType"] = [exception.GetType().FullName ?? exception.GetType().Name],
                    ["TraceId"] = [Activity.Current?.Id ?? httpContext.TraceIdentifier]
                })
            : Error.Unexpected(
                "unexpected_error",
                "An unexpected error occurred.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(Result.Failure(error), cancellationToken);

        return true;
    }

    private static bool TryMapToValidationError(Exception exception, out Error error)
    {
        if (exception is not BadHttpRequestException badHttpRequestException)
        {
            error = default!;
            return false;
        }

        if (badHttpRequestException.InnerException is JsonException jsonException)
        {
            error = CreateJsonValidationError(jsonException);
            return true;
        }

        error = Error.Validation(
            "invalid_request_body",
            "Request is invalid.",
            new Dictionary<string, string[]>
            {
                ["Request"] = [badHttpRequestException.Message]
            });

        return true;
    }

    private static Error CreateJsonValidationError(JsonException jsonException)
    {
        var fieldName = MapFieldName(jsonException.Path);
        var fieldMessage = fieldName == nameof(UpsertTaskItemRequest.DueDate)
            ? "DueDate must be a valid ISO 8601 date, for example 2026-04-10 or 2026-04-10T14:30:00Z."
            : $"Invalid value supplied for '{fieldName}'. Check the JSON type and format.";

        return Error.Validation(
            "invalid_request_body",
            "Request body contains invalid JSON or invalid field values.",
            new Dictionary<string, string[]>
            {
                [fieldName] = [fieldMessage]
            });
    }

    private static string MapFieldName(string? jsonPath)
    {
        if (string.IsNullOrWhiteSpace(jsonPath))
        {
            return "Request";
        }

        var rawFieldName = jsonPath.Trim()
            .TrimStart('$')
            .TrimStart('.')
            .Split('.', '[', ']')[0];

        return rawFieldName.ToLowerInvariant() switch
        {
            "title" => nameof(UpsertTaskItemRequest.Title),
            "description" => nameof(UpsertTaskItemRequest.Description),
            "status" => nameof(UpsertTaskItemRequest.Status),
            "duedate" => nameof(UpsertTaskItemRequest.DueDate),
            _ => "Request"
        };
    }
}
