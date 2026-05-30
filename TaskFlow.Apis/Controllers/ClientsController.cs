using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Clients;
using TaskFlow.Application.Features.Clients.Commands.CreateClient;
using TaskFlow.Application.Features.Clients.Commands.DeleteClient;
using TaskFlow.Application.Features.Clients.Commands.UpdateClient;
using TaskFlow.Application.Features.Clients.Queries.GetClientById;
using TaskFlow.Application.Features.Clients.Queries.GetClients;
using TaskFlow.Shared.Common;

namespace TaskFlow.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ClientDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClients(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetClientsQuery(pageNumber, pageSize), cancellationToken);
        return Ok(ApiResponse<PaginatedList<ClientDto>>.FromResult(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClientByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<ClientDto>.FromResult(result))
            : NotFound(ApiResponse.Fail(result.Error.Description));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Analyst")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateClientCommand(request.Name, request.Email, request.Phone, request.Company, request.Notes);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetClientById), new { id = result.Value }, ApiResponse<Guid>.Ok(result.Value))
            : BadRequest(ApiResponse.Fail(result.Error.Description));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Analyst")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClient(Guid id, [FromBody] UpdateClientRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateClientCommand(id, request.Name, request.Phone, request.Company, request.Notes);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Client updated successfully."))
            : MapErrorToResponse(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClient(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteClientCommand(id), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Client deleted successfully."))
            : NotFound(ApiResponse.Fail(result.Error.Description));
    }

    private IActionResult MapErrorToResponse(Error error) => error.Code switch
    {
        "Error.NotFound" => NotFound(ApiResponse.Fail(error.Description)),
        _ => BadRequest(ApiResponse.Fail(error.Description))
    };
}
