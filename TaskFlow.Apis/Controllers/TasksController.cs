using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Comments;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Features.Comments.Commands.AddComment;
using TaskFlow.Application.Features.Comments.Commands.DeleteComment;
using TaskFlow.Application.Features.Comments.Queries.GetComments;
using TaskFlow.Application.Features.Tasks.Commands.CreateTask;
using TaskFlow.Application.Features.Tasks.Commands.DeleteTask;
using TaskFlow.Application.Features.Tasks.Commands.UpdateTask;
using TaskFlow.Application.Features.Tasks.Queries.GetTaskById;
using TaskFlow.Application.Features.Tasks.Queries.GetTasksByBoard;
using TaskFlow.Application.Features.Tasks.Queries.GetTasksByBoardWithCursor;
using TaskFlow.Application.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public TasksController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<TaskDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasksByBoard(
        [FromQuery] Guid boardId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetTasksByBoardQuery(boardId, _currentUser.UserId, pageNumber, pageSize), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<PaginatedList<TaskDto>>.FromResult(result))
            : MapErrorToResponse(result.Error);
    }

    /// <summary>
    /// Paginación por cursores (keyset pagination).
    /// Más eficiente que offset para tablas grandes y evita drift en tiempo real.
    /// Usar el campo nextCursor de la respuesta para obtener la siguiente página.
    /// </summary>
    [HttpGet("cursor")]
    [ProducesResponseType(typeof(ApiResponse<CursorPaginatedList<TaskDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasksByBoardWithCursor(
        [FromQuery] Guid boardId,
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetTasksByBoardWithCursorQuery(boardId, _currentUser.UserId, cursor, pageSize), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<CursorPaginatedList<TaskDto>>.FromResult(result))
            : MapErrorToResponse(result.Error);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTaskByIdQuery(id, _currentUser.UserId), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<TaskDto>.FromResult(result))
            : MapErrorToResponse(result.Error);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromQuery] Guid boardId, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            boardId,
            request.AssigneeId,
            _currentUser.UserId);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetTaskById), new { id = result.Value }, ApiResponse<Guid>.Ok(result.Value))
            : MapErrorToResponse(result.Error);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand(
            id,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.DueDate,
            request.AssigneeId,
            _currentUser.UserId);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Task updated successfully."))
            : MapErrorToResponse(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteTaskCommand(id, _currentUser.UserId), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Task deleted successfully."))
            : MapErrorToResponse(result.Error);
    }

    // ── Comments (sub-recurso de Task) ─────────────────────────────────────────

    [HttpGet("{taskId:guid}/comments")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<CommentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(
        Guid taskId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCommentsQuery(taskId, pageNumber, pageSize), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<PaginatedList<CommentDto>>.FromResult(result))
            : MapErrorToResponse(result.Error);
    }

    [HttpPost("{taskId:guid}/comments")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddComment(
        Guid taskId,
        [FromBody] AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddCommentCommand(taskId, request.Content, _currentUser.UserId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetComments), new { taskId }, ApiResponse<Guid>.Ok(result.Value))
            : MapErrorToResponse(result.Error);
    }

    [HttpDelete("{taskId:guid}/comments/{commentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(
        Guid taskId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCommentCommand(commentId, _currentUser.UserId), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Comment deleted successfully."))
            : MapErrorToResponse(result.Error);
    }

    private IActionResult MapErrorToResponse(Error error) => error.Code switch
    {
        "Error.NotFound" => NotFound(ApiResponse.Fail(error.Description)),
        "Error.Forbidden" => StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Fail(error.Description)),
        _ => BadRequest(ApiResponse.Fail(error.Description))
    };
}
