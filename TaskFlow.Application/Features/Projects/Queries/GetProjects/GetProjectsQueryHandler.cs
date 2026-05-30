using MediatR;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Projects.Queries.GetProjects;

public sealed class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, Result<PaginatedList<ProjectDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedList<ProjectDto>>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var (projects, total) = await _unitOfWork.Projects.GetByAnalystIdPagedAsync(
            request.AnalystId, request.PageNumber, request.PageSize, cancellationToken);

        var dtos = projects.Select(p => new ProjectDto(
            p.Id,
            p.Title,
            p.Description,
            p.Status,
            p.StartDate,
            p.EndDate,
            p.ClientId,
            p.Client.Name,
            p.AnalystId,
            p.Analyst.FullName,
            p.Boards.Count,
            p.CreatedAt,
            p.UpdatedAt)).ToList();

        return Result.Success(new PaginatedList<ProjectDto>(dtos, total, request.PageNumber, request.PageSize));
    }
}
