using MediatR;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Projects.Queries.GetProjectById;

public sealed class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdWithBoardsAsync(request.Id, cancellationToken);

        if (project is null)
            return Result.Failure<ProjectDto>(Error.NotFound);

        return Result.Success(new ProjectDto(
            project.Id,
            project.Title,
            project.Description,
            project.Status,
            project.StartDate,
            project.EndDate,
            project.ClientId,
            project.Client.Name,
            project.AnalystId,
            project.Analyst.FullName,
            project.Boards.Count,
            project.CreatedAt,
            project.UpdatedAt));
    }
}
