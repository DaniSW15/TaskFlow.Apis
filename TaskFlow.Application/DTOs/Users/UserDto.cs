using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Users;

public sealed record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    UserRole Role,
    DateTime CreatedAt,
    DateTime UpdatedAt);
