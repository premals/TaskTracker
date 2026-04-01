using FluentValidation;
using FluentValidation.Results;
using TaskTracker.Requests;

namespace TaskTracker.Validators;

public sealed class UpsertTaskItemRequestValidator : AbstractValidator<UpsertTaskItemRequest>
{
    public UpsertTaskItemRequestValidator()
    {
        RuleFor(request => request.Status)
            .Must(status => TaskRequestParsing.TryParseStatus(status, out _))
            .WithMessage("Status must be one of Todo, InProgress, or Done.")
            .WithErrorCode("validation_error");

        RuleFor(request => request.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title is required and cannot be whitespace.")
            .WithErrorCode("validation_error")
            .When(request => !TaskRequestParsing.IsDoneStatus(request.Status));

        RuleFor(request => request.Title)
            .Must(title => string.IsNullOrWhiteSpace(title) || title.Trim().Length <= 100)
            .WithMessage("Title must not exceed 100 characters.")
            .WithErrorCode("validation_error");

        RuleFor(request => request.DueDate)
            .Must(dueDate => TaskRequestParsing.TryParseDueDate(dueDate, out _))
            .WithMessage(TaskRequestParsing.DueDateValidationMessage)
            .WithErrorCode("validation_error");

        RuleFor(request => request)
            .Custom(AddCompletedTaskTitleRule);
    }

    private static void AddCompletedTaskTitleRule(
        UpsertTaskItemRequest request,
        ValidationContext<UpsertTaskItemRequest> context)
    {
        if (!TaskRequestParsing.IsDoneStatus(request.Status) || !string.IsNullOrWhiteSpace(request.Title))
        {
            return;
        }

        context.AddFailure(new ValidationFailure(
            nameof(UpsertTaskItemRequest.Status),
            "A task cannot be marked as Done when the title is empty or whitespace.")
        {
            ErrorCode = "task_done_requires_title"
        });

        context.AddFailure(new ValidationFailure(
            nameof(UpsertTaskItemRequest.Title),
            "Title is required when a task is marked as Done.")
        {
            ErrorCode = "task_done_requires_title"
        });
    }
}
