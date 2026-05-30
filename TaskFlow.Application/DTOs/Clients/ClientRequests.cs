namespace TaskFlow.Application.DTOs.Clients;

public sealed record CreateClientRequest(
    string Name,
    string Email,
    string? Phone,
    string? Company,
    string? Notes);

public sealed record UpdateClientRequest(
    string Name,
    string? Phone,
    string? Company,
    string? Notes);
