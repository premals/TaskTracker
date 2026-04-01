using FluentValidation;
using TaskTracker.Common;
using TaskTracker.Models;
using TaskTracker.Repositories;
using TaskTracker.Requests;
using TaskTracker.Responses;
using TaskTracker.Validators;

namespace TaskTracker.Services;

public sealed class TaskService(
    IGenericRepository<TaskItem> repository,
    IValidator<UpsertTaskItemRequest> validator) : ITaskService
{
    public async Task<Result<TaskItemResponse>> CreateAsync(
        UpsertTaskItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResult<TaskItemResponse>();
        }

        var input = MapValidatedInput(request);
        var taskItem = new TaskItem
        {
            Title = input.Title,
            Description = input.Description,
            Status = input.Status,
            DueDate = input.DueDate
        };

        await repository.AddAsync(taskItem, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<TaskItemResponse>.Success(MapToResponse(taskItem), "Task created successfully.");
    }

    public async Task<Result<IReadOnlyCollection<TaskItemResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var taskItems = await repository.GetAllAsync(cancellationToken);
        var response = taskItems
            .OrderBy(task => task.Id)
            .Select(MapToResponse)
            .ToArray();

        return Result<IReadOnlyCollection<TaskItemResponse>>.Success(response);
    }

    public async Task<Result<TaskItemResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var taskItem = await repository.GetByIdAsync(id, cancellationToken);
        if (taskItem is null)
        {
            return Result<TaskItemResponse>.Failure(
                Error.NotFound("task_not_found", $"Task with id '{id}' was not found."));
        }

        return Result<TaskItemResponse>.Success(MapToResponse(taskItem));
    }

    public async Task<Result<TaskItemResponse>> UpdateAsync(
        int id,
        UpsertTaskItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResult<TaskItemResponse>();
        }

        var taskItem = await repository.GetByIdAsync(id, cancellationToken);
        if (taskItem is null)
        {
            return Result<TaskItemResponse>.Failure(
                Error.NotFound("task_not_found", $"Task with id '{id}' was not found."));
        }

        var input = MapValidatedInput(request);
        taskItem.Title = input.Title;
        taskItem.Description = input.Description;
        taskItem.Status = input.Status;
        taskItem.DueDate = input.DueDate;

        repository.Update(taskItem);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<TaskItemResponse>.Success(MapToResponse(taskItem), "Task updated successfully.");
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var taskItem = await repository.GetByIdAsync(id, cancellationToken);
        if (taskItem is null)
        {
            return Result.Failure(Error.NotFound("task_not_found", $"Task with id '{id}' was not found."));
        }

        repository.Delete(taskItem);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success("Task deleted successfully.");
    }

    private static TaskItemResponse MapToResponse(TaskItem taskItem) =>
        new()
        {
            Id = taskItem.Id,
            Title = taskItem.Title,
            Description = taskItem.Description,
            Status = taskItem.Status,
            DueDate = taskItem.DueDate
        };

    private static ValidatedTaskItemInput MapValidatedInput(UpsertTaskItemRequest request)
    {
        TaskRequestParsing.TryParseStatus(request.Status, out var status);
        TaskRequestParsing.TryParseDueDate(request.DueDate, out var dueDate);

        return new ValidatedTaskItemInput(
            request.Title!.Trim(),
            string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            status,
            dueDate);
    }

    private sealed record ValidatedTaskItemInput(
        string Title,
        string? Description,
        TaskItemStatus Status,
        DateTime? DueDate);
}
