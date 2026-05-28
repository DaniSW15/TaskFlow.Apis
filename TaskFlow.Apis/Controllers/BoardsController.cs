using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Boards;
using TaskFlow.Application.Features.Boards.Commands.CreateBoard;
using TaskFlow.Application.Features.Boards.Commands.DeleteBoard;
using TaskFlow.Application.Features.Boards.Commands.UpdateBoard;
using TaskFlow.Application.Features.Boards.Queries.GetBoardById;
using TaskFlow.Application.Features.Boards.Queries.GetBoards;
using TaskFlow.Application.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class BoardsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public BoardsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<BoardDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBoards(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetBoardsQuery(_currentUser.UserId, pageNumber, pageSize), cancellationToken);
        return Ok(ApiResponse<PaginatedList<BoardDto>>.FromResult(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BoardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBoardById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBoardByIdQuery(id, _currentUser.UserId), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<BoardDto>.FromResult(result))
            : MapErrorToResponse(result.Error);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateBoardCommand(request.Title, request.Description, _currentUser.UserId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetBoardById), new { id = result.Value }, ApiResponse<Guid>.Ok(result.Value))
            : BadRequest(ApiResponse.Fail(result.Error.Description));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBoardCommand(id, request.Title, request.Description, _currentUser.UserId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Board updated successfully."))
            : MapErrorToResponse(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBoard(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteBoardCommand(id, _currentUser.UserId), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Board deleted successfully."))
            : MapErrorToResponse(result.Error);
    }

    private IActionResult MapErrorToResponse(Error error) => error.Code switch
    {
        "Error.NotFound" => NotFound(ApiResponse.Fail(error.Description)),
        "Error.Forbidden" => StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Fail(error.Description)),
        _ => BadRequest(ApiResponse.Fail(error.Description))
    };
}
