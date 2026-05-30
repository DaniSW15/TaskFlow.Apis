using MediatR;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Projects.Queries.GetProjects;

public sealed record GetProjectsQuery(
    Guid AnalystId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PaginatedList<ProjectDto>>>;
