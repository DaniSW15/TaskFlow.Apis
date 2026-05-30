using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Users;

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName);

public sealed record UpdateUserRoleRequest(UserRole Role);
