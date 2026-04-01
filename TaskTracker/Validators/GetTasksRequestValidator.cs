using FluentValidation;
using TaskTracker.Requests;

namespace TaskTracker.Validators;

public sealed class GetTasksRequestValidator : AbstractValidator<GetTasksRequest>
{
    public GetTasksRequestValidator()
    {
        RuleFor(request => request.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.")
            .WithErrorCode("validation_error");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.")
            .WithErrorCode("validation_error");
    }
}
