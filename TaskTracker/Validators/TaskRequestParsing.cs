using System.Globalization;
using TaskTracker.Models;

namespace TaskTracker.Validators;

public static class TaskRequestParsing
{
    private static readonly string[] SupportedDueDateFormats =
    [
        "yyyy-MM-dd",
        "yyyy-MM-ddTHH:mm",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
        "yyyy-MM-ddTHH:mmK",
        "yyyy-MM-ddTHH:mm:ssK",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss.FFFFFFF"
    ];

    public const string DueDateValidationMessage =
        "DueDate must be a valid ISO 8601 date, for example 2026-04-10 or 2026-04-10T14:30:00Z.";

    public static bool TryParseStatus(string? statusValue, out TaskItemStatus status)
    {
        if (string.IsNullOrWhiteSpace(statusValue))
        {
            status = TaskItemStatus.Todo;
            return true;
        }

        if (Enum.TryParse<TaskItemStatus>(statusValue, true, out status) &&
            Enum.IsDefined(status))
        {
            return true;
        }

        status = TaskItemStatus.Todo;
        return false;
    }

    public static bool IsDoneStatus(string? statusValue) =>
        TryParseStatus(statusValue, out var status) && status == TaskItemStatus.Done;

    public static bool TryParseDueDate(string? dueDateValue, out DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(dueDateValue))
        {
            dueDate = null;
            return true;
        }

        if (DateTime.TryParseExact(
                dueDateValue.Trim(),
                SupportedDueDateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
                out var parsedDueDate))
        {
            dueDate = parsedDueDate;
            return true;
        }

        dueDate = null;
        return false;
    }
}
