namespace TaskFlow.Application.DTOs.Clients;

public sealed record ClientDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Company,
    string? Notes,
    int ProjectCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
