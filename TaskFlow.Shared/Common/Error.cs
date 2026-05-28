namespace TaskFlow.Shared.Common;

public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");
    public static readonly Error Unauthorized = new("Error.Unauthorized", "Unauthorized access.");
    public static readonly Error Forbidden = new("Error.Forbidden", "You do not have permission to perform this action.");
    public static readonly Error NotFound = new("Error.NotFound", "The requested resource was not found.");

    public static Error Validation(string description) => new("Error.Validation", description);
    public static Error Conflict(string description) => new("Error.Conflict", description);
    public static Error Custom(string code, string description) => new(code, description);
}
