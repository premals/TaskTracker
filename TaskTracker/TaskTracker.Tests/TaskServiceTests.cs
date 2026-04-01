using TaskTracker.Models;
using TaskTracker.Repositories;
using TaskTracker.Requests;
using TaskTracker.Responses;
using TaskTracker.Services;
using TaskTracker.Validators;

namespace TaskTracker.Tests;

public sealed class TaskServiceTests
{
    [Fact]
    public async Task CreateAsync_ReturnsValidationFailure_WhenTitleIsWhitespace()
    {
        var repository = new InMemoryGenericRepository<TaskItem>();
        var service = CreateService(repository);

        var result = await service.CreateAsync(new UpsertTaskItemRequest
        {
            Title = "   ",
            Status = "Todo"
        });

        Assert.False(result.Succeeded);
        Assert.Equal("validation_error", result.Error?.Code);
        Assert.Contains("Title", result.Error?.Details?.Keys ?? []);
    }

    [Fact]
    public async Task CreateAsync_ReturnsValidationFailure_WhenTitleExceedsMaxLength()
    {
        var repository = new InMemoryGenericRepository<TaskItem>();
        var service = CreateService(repository);

        var result = await service.CreateAsync(new UpsertTaskItemRequest
        {
            Title = new string('A', 101),
            Status = "InProgress"
        });

        Assert.False(result.Succeeded);
        Assert.Equal("validation_error", result.Error?.Code);
        Assert.Contains("Title", result.Error?.Details?.Keys ?? []);
    }

    [Fact]
    public async Task CreateAsync_ReturnsValidationFailure_WhenDueDateFormatIsInvalid()
    {
        var repository = new InMemoryGenericRepository<TaskItem>();
        var service = CreateService(repository);

        var result = await service.CreateAsync(new UpsertTaskItemRequest
        {
            Title = "Task with bad date",
            DueDate = "04/31/2026"
        });

        Assert.False(result.Succeeded);
        Assert.Equal("validation_error", result.Error?.Code);
        Assert.Contains("DueDate", result.Error?.Details?.Keys ?? []);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsBusinessValidationFailure_WhenDoneHasWhitespaceTitle()
    {
        var repository = new InMemoryGenericRepository<TaskItem>();
        repository.Seed(new TaskItem
        {
            Id = 7,
            Title = "Existing task",
            Status = TaskItemStatus.Todo
        });

        var service = CreateService(repository);

        var result = await service.UpdateAsync(7, new UpsertTaskItemRequest
        {
            Title = "   ",
            Status = "Done"
        });

        Assert.False(result.Succeeded);
        Assert.Equal("task_done_requires_title", result.Error?.Code);
        Assert.Contains("Status", result.Error?.Details?.Keys ?? []);
    }

    [Fact]
    public async Task CreateAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        var repository = new InMemoryGenericRepository<TaskItem>();
        var service = CreateService(repository);

        var result = await service.CreateAsync(new UpsertTaskItemRequest
        {
            Title = "Ship release",
            Description = "Prepare release notes",
            Status = "InProgress",
            DueDate = "2026-04-10"
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data!.Id);
        Assert.Equal(TaskItemStatus.InProgress, result.Data.Status);
        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        var repository = new InMemoryGenericRepository<TaskItem>();
        repository.Seed(new TaskItem
        {
            Id = 3,
            Title = "Old title",
            Description = "Old description",
            Status = TaskItemStatus.Todo
        });

        var service = CreateService(repository);

        var result = await service.UpdateAsync(3, new UpsertTaskItemRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            Status = "Done",
            DueDate = "2026-04-15"
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Updated title", result.Data!.Title);
        Assert.Equal(TaskItemStatus.Done, result.Data.Status);

        var updatedEntity = Assert.Single(repository.Items);
        Assert.Equal("Updated title", updatedEntity.Title);
        Assert.Equal("Updated description", updatedEntity.Description);
        Assert.Equal(TaskItemStatus.Done, updatedEntity.Status);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedTasks_WhenPaginationIsValid()
    {
        var repository = new InMemoryGenericRepository<TaskItem>();
        repository.Seed(new TaskItem { Id = 1, Title = "Task 1", Status = TaskItemStatus.Todo });
        repository.Seed(new TaskItem { Id = 2, Title = "Task 2", Status = TaskItemStatus.Todo });
        repository.Seed(new TaskItem { Id = 3, Title = "Task 3", Status = TaskItemStatus.Todo });

        var service = CreateService(repository);

        var result = await service.GetAllAsync(new GetTasksRequest
        {
            PageNumber = 2,
            PageSize = 1
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.PageNumber);
        Assert.Equal(1, result.Data.PageSize);
        Assert.Equal(3, result.Data.TotalCount);
        Assert.Equal(3, result.Data.TotalPages);

        var item = Assert.Single(result.Data.Items);
        Assert.Equal(2, item.Id);
        Assert.True(result.Data.HasPreviousPage);
        Assert.True(result.Data.HasNextPage);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsValidationFailure_WhenPageSizeIsOutOfRange()
    {
        var repository = new InMemoryGenericRepository<TaskItem>();
        var service = CreateService(repository);

        var result = await service.GetAllAsync(new GetTasksRequest
        {
            PageNumber = 1,
            PageSize = 101
        });

        Assert.False(result.Succeeded);
        Assert.Equal("validation_error", result.Error?.Code);
        Assert.Contains("PageSize", result.Error?.Details?.Keys ?? []);
    }

    private static TaskService CreateService(InMemoryGenericRepository<TaskItem> repository) =>
        new(
            repository,
            new UpsertTaskItemRequestValidator(),
            new GetTasksRequestValidator());

    private sealed class InMemoryGenericRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class
    {
        private readonly List<TEntity> _items = [];
        private int _nextId = 1;

        public IReadOnlyList<TEntity> Items => _items;

        public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.ToList());

        public Task<List<TEntity>> GetPagedAsync<TOrderKey>(
            int pageNumber,
            int pageSize,
            System.Linq.Expressions.Expression<Func<TEntity, TOrderKey>> orderBy,
            CancellationToken cancellationToken = default)
        {
            var keySelector = orderBy.Compile();
            var items = _items
                .OrderBy(keySelector)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(items);
        }

        public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Count);

        public ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            var match = _items.SingleOrDefault(item => GetId(item) == Convert.ToInt32(id));
            return ValueTask.FromResult(match);
        }

        public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (GetId(entity) == 0)
            {
                SetId(entity, _nextId++);
            }
            else
            {
                _nextId = Math.Max(_nextId, GetId(entity) + 1);
            }

            _items.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(TEntity entity)
        {
        }

        public void Delete(TEntity entity) => _items.Remove(entity);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public void Seed(TEntity entity)
        {
            if (GetId(entity) == 0)
            {
                SetId(entity, _nextId++);
            }
            else
            {
                _nextId = Math.Max(_nextId, GetId(entity) + 1);
            }

            _items.Add(entity);
        }

        private static int GetId(TEntity entity) =>
            entity is TaskItem taskItem
                ? taskItem.Id
                : throw new InvalidOperationException("This test repository expects TaskItem entities.");

        private static void SetId(TEntity entity, int id)
        {
            if (entity is TaskItem taskItem)
            {
                taskItem.Id = id;
                return;
            }

            throw new InvalidOperationException("This test repository expects TaskItem entities.");
        }
    }
}
