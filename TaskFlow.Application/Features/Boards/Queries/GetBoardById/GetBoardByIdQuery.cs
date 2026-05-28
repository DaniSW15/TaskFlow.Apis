using MediatR;
using TaskFlow.Application.DTOs.Boards;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Boards.Queries.GetBoardById;

public sealed record GetBoardByIdQuery(Guid Id, Guid RequestingUserId) : IRequest<Result<BoardDto>>;
