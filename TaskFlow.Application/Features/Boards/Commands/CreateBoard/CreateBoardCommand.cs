using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Boards.Commands.CreateBoard;

public sealed record CreateBoardCommand(
    string Title,
    string? Description,
    Guid OwnerId) : IRequest<Result<Guid>>;
