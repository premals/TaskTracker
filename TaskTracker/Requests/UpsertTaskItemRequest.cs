namespace TaskTracker.Requests;

public sealed class UpsertTaskItemRequest
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Status { get; init; }

    public string? DueDate { get; init; }
}
