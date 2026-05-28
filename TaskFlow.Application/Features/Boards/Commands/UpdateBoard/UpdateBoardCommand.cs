using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Boards.Commands.UpdateBoard;

public sealed record UpdateBoardCommand(
    Guid Id,
    string Title,
    string? Description,
    Guid RequestingUserId) : IRequest<Result>;
