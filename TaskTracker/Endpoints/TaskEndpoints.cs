using TaskTracker.Common;
using TaskTracker.Requests;
using TaskTracker.Responses;
using TaskTracker.Services;

namespace TaskTracker.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/tasks");

        group.MapPost("/", CreateTaskAsync);
        group.MapGet("/", GetTasksAsync);
        group.MapGet("/{id:int}", GetTaskByIdAsync);
        group.MapPut("/{id:int}", UpdateTaskAsync);
        group.MapDelete("/{id:int}", DeleteTaskAsync);

        return endpoints;
    }

    private static async Task<IResult> CreateTaskAsync(
        UpsertTaskItemRequest? request,
        ITaskService taskService,
        CancellationToken cancellationToken)
    {
        var result = request is null
            ? ResultExtensions.MissingRequestBody<TaskItemResponse>()
            : await taskService.CreateAsync(request, cancellationToken);

        return result.ToHttpResult(success => Results.Created($"/tasks/{success.Data!.Id}", success));
    }

    private static async Task<IResult> GetTasksAsync(
        ITaskService taskService,
        CancellationToken cancellationToken,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result = await taskService.GetAllAsync(
            new GetTasksRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> GetTaskByIdAsync(
        int id,
        ITaskService taskService,
        CancellationToken cancellationToken)
    {
        var result = await taskService.GetByIdAsync(id, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateTaskAsync(
        int id,
        UpsertTaskItemRequest? request,
        ITaskService taskService,
        CancellationToken cancellationToken)
    {
        var result = request is null
            ? ResultExtensions.MissingRequestBody<TaskItemResponse>()
            : await taskService.UpdateAsync(id, request, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteTaskAsync(
        int id,
        ITaskService taskService,
        CancellationToken cancellationToken)
    {
        var result = await taskService.DeleteAsync(id, cancellationToken);
        return result.ToHttpResult();
    }
}
