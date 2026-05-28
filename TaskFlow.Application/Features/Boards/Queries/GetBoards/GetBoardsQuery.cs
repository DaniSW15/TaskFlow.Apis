using MediatR;
using TaskFlow.Application.DTOs.Boards;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Boards.Queries.GetBoards;

public sealed record GetBoardsQuery(
    Guid OwnerId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<BoardDto>>>;
