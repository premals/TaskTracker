using TaskTracker.Models;

namespace TaskTracker.Responses;

public sealed class TaskItemResponse
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public TaskItemStatus Status { get; init; }

    public DateTime? DueDate { get; init; }
}
