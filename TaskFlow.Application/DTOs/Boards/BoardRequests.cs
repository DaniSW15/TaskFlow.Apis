namespace TaskFlow.Application.DTOs.Boards;

public sealed record CreateBoardRequest(string Title, string? Description);

public sealed record UpdateBoardRequest(string Title, string? Description);
