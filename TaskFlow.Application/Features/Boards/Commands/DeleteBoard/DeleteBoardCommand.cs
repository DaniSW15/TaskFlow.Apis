using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Boards.Commands.DeleteBoard;

public sealed record DeleteBoardCommand(Guid Id, Guid RequestingUserId) : IRequest<Result>;
