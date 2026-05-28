using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Boards;

public sealed record BoardDto(
    Guid Id,
    string Title,
    string? Description,
    Guid OwnerId,
    int TaskCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
