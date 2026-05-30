using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Tags;
using TaskFlow.Application.Features.Tags.Commands.AddTagToTask;
using TaskFlow.Application.Features.Tags.Commands.CreateTag;
using TaskFlow.Application.Features.Tags.Commands.DeleteTag;
using TaskFlow.Application.Features.Tags.Commands.RemoveTagFromTask;
using TaskFlow.Application.Features.Tags.Queries.GetTags;
using TaskFlow.Shared.Common;

namespace TaskFlow.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene todos los tags disponibles en el sistema.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TagDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTags(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTagsQuery(), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<TagDto>>.FromResult(result))
            : BadRequest(ApiResponse.Fail(result.Error.Description));
    }

    /// <summary>
    /// Crea un nuevo tag global. Solo Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateTagCommand(request.Name, request.Color), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetTags), null, ApiResponse<Guid>.Ok(result.Value))
            : MapErrorToResponse(result.Error);
    }

    /// <summary>
    /// Elimina un tag global. Solo Admin.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteTagCommand(id), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Tag deleted successfully."))
            : MapErrorToResponse(result.Error);
    }

    // ── Relación N:M: Tags ↔ TaskItems ────────────────────────────────────────

    /// <summary>
    /// Asocia un tag a una tarea. Escribe en la tabla TaskItemTags.
    /// </summary>
    [HttpPost("/api/tasks/{taskId:guid}/tags/{tagId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTagToTask(Guid taskId, Guid tagId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddTagToTaskCommand(taskId, tagId), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Tag added to task."))
            : MapErrorToResponse(result.Error);
    }

    /// <summary>
    /// Desasocia un tag de una tarea. Borra la fila de TaskItemTags.
    /// </summary>
    [HttpDelete("/api/tasks/{taskId:guid}/tags/{tagId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTagFromTask(Guid taskId, Guid tagId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RemoveTagFromTaskCommand(taskId, tagId), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Tag removed from task."))
            : MapErrorToResponse(result.Error);
    }

    private IActionResult MapErrorToResponse(Error error) => error.Code switch
    {
        "Error.NotFound" => NotFound(ApiResponse.Fail(error.Description)),
        "Error.Forbidden" => StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Fail(error.Description)),
        _ => BadRequest(ApiResponse.Fail(error.Description))
    };
}
