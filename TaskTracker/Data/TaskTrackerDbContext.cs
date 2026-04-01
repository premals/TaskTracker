using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;

namespace TaskTracker.Data;

public sealed class TaskTrackerDbContext(DbContextOptions<TaskTrackerDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(task => task.Id);
            entity.Property(task => task.Title)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(task => task.Status)
                .HasConversion<string>()
                .HasMaxLength(25);
        });
    }
}
