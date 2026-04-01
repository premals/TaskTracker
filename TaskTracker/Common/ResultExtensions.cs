namespace TaskTracker.Common;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<Result<T>, IResult>? onSuccess = null)
    {
        if (result.Succeeded)
        {
            return onSuccess is null ? Results.Ok(result) : onSuccess(result);
        }

        return CreateFailureResponse(result);
    }

    public static IResult ToHttpResult(this Result result, Func<Result, IResult>? onSuccess = null)
    {
        if (result.Succeeded)
        {
            return onSuccess is null ? Results.Ok(result) : onSuccess(result);
        }

        return CreateFailureResponse(result);
    }

    public static Result<T> MissingRequestBody<T>() =>
        Result<T>.Failure(
            Error.Validation(
                "request_body_required",
                "Request body is required.",
                new Dictionary<string, string[]>
                {
                    ["Request"] = ["Request body is required."]
                }));

    private static IResult CreateFailureResponse(Result result)
    {
        var statusCode = result.Error?.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Json(result, statusCode: statusCode);
    }
}
