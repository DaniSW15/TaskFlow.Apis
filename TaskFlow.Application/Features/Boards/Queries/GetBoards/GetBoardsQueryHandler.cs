using MediatR;
using TaskFlow.Application.DTOs.Boards;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Boards.Queries.GetBoards;

public sealed class GetBoardsQueryHandler : IRequestHandler<GetBoardsQuery, Result<PaginatedList<BoardDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBoardsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedList<BoardDto>>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
    {
        var (boards, total) = await _unitOfWork.Boards.GetByOwnerIdPagedAsync(
            request.OwnerId, request.PageNumber, request.PageSize, cancellationToken);

        var dtos = boards.Select(b => new BoardDto(
            b.Id,
            b.Title,
            b.Description,
            b.OwnerId,
            b.Tasks.Count,
            b.CreatedAt,
            b.UpdatedAt)).ToList();

        return Result.Success(new PaginatedList<BoardDto>(dtos, total, request.PageNumber, request.PageSize));
    }
}
