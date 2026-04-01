using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Common;
using TaskTracker.Data;
using TaskTracker.Endpoints;
using TaskTracker.Exceptions;
using TaskTracker.Repositories;
using TaskTracker.Services;
using TaskTracker.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<UpsertTaskItemRequestValidator>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("TaskTracker") ?? "Data Source=tasktracker.db");
});
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITaskService, TaskService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseExceptionHandler();

await app.InitializeDatabaseAsync();

app.MapTaskEndpoints();

app.Run();

public partial class Program;
