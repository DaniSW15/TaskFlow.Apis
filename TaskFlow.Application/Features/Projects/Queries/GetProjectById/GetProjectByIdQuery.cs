using MediatR;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Projects.Queries.GetProjectById;

public sealed record GetProjectByIdQuery(Guid Id) : IRequest<Result<ProjectDto>>;
