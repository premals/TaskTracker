namespace TaskTracker.Requests;

public sealed class GetTasksRequest
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
