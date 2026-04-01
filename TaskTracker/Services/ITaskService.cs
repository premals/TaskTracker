using TaskTracker.Common;
using TaskTracker.Requests;
using TaskTracker.Responses;

namespace TaskTracker.Services;

public interface ITaskService
{
    Task<Result<TaskItemResponse>> CreateAsync(UpsertTaskItemRequest request, CancellationToken cancellationToken = default);

    Task<Result<PagedResponse<TaskItemResponse>>> GetAllAsync(GetTasksRequest request, CancellationToken cancellationToken = default);

    Task<Result<TaskItemResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<TaskItemResponse>> UpdateAsync(int id, UpsertTaskItemRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
