using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
    UserRole Role { get; }
    bool IsAdmin { get; }
}
