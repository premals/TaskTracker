using Microsoft.EntityFrameworkCore;

namespace TaskTracker.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
