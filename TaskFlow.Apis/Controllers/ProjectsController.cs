using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.Features.Projects.Commands.CreateProject;
using TaskFlow.Application.Features.Projects.Commands.DeleteProject;
using TaskFlow.Application.Features.Projects.Commands.UpdateProject;
using TaskFlow.Application.Features.Projects.Queries.GetProjectById;
using TaskFlow.Application.Features.Projects.Queries.GetProjects;
using TaskFlow.Application.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public ProjectsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ProjectDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjects(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetProjectsQuery(_currentUser.UserId, pageNumber, pageSize), cancellationToken);
        return Ok(ApiResponse<PaginatedList<ProjectDto>>.FromResult(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<ProjectDto>.FromResult(result))
            : NotFound(ApiResponse.Fail(result.Error.Description));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Analyst")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand(
            request.Title, request.Description,
            request.ClientId, request.AnalystId,
            request.StartDate, request.EndDate);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetProjectById), new { id = result.Value }, ApiResponse<Guid>.Ok(result.Value))
            : BadRequest(ApiResponse.Fail(result.Error.Description));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Analyst")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProjectCommand(
            id, request.Title, request.Description,
            request.Status, request.StartDate, request.EndDate);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Project updated successfully."))
            : MapErrorToResponse(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteProjectCommand(id), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Project deleted successfully."))
            : NotFound(ApiResponse.Fail(result.Error.Description));
    }

    private IActionResult MapErrorToResponse(Error error) => error.Code switch
    {
        "Error.NotFound" => NotFound(ApiResponse.Fail(error.Description)),
        _ => BadRequest(ApiResponse.Fail(error.Description))
    };
}
