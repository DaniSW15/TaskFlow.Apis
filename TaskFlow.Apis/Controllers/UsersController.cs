using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Application.Features.Users.Commands.DeleteUser;
using TaskFlow.Application.Features.Users.Commands.UpdateUser;
using TaskFlow.Application.Features.Users.Commands.UpdateUserRole;
using TaskFlow.Application.Features.Users.Queries.GetUserById;
using TaskFlow.Application.Features.Users.Queries.GetUsers;
using TaskFlow.Application.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // todos los endpoints requieren estar autenticado mínimo
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    // GET /api/users/me — cualquier usuario logueado ve su propio perfil
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(_currentUser.UserId), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<UserDto>.FromResult(result))
            : NotFound(ApiResponse.Fail(result.Error.Description));
    }

    // GET /api/users — solo Admin puede listar todos los usuarios
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUsersQuery(pageNumber, pageSize), cancellationToken);
        return Ok(ApiResponse<PaginatedList<UserDto>>.FromResult(result));
    }

    // GET /api/users/{id} — solo Admin puede ver cualquier usuario
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<UserDto>.FromResult(result))
            : NotFound(ApiResponse.Fail(result.Error.Description));
    }

    // PUT /api/users/me — cualquier usuario puede editar su propio nombre
    [HttpPut("me")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(
            _currentUser.UserId, request.FirstName, request.LastName, _currentUser.UserId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Profile updated successfully."))
            : BadRequest(ApiResponse.Fail(result.Error.Description));
    }

    // PATCH /api/users/{id}/role — solo Admin puede cambiar el rol de un usuario
    [HttpPatch("{id:guid}/role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateUserRoleCommand(id, request.Role), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("User role updated successfully."))
            : NotFound(ApiResponse.Fail(result.Error.Description));
    }

    // DELETE /api/users/{id} — solo Admin puede eliminar usuarios
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("User deleted successfully."))
            : NotFound(ApiResponse.Fail(result.Error.Description));
    }
}
