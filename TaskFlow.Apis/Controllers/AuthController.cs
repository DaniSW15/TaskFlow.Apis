using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Features.Auth.Commands.Login;
using TaskFlow.Application.Features.Auth.Commands.Logout;
using TaskFlow.Application.Features.Auth.Commands.RefreshToken;
using TaskFlow.Application.Features.Auth.Commands.Register;
using TaskFlow.Application.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Register), ApiResponse<AuthResponse>.Ok(result.Value, "Account created successfully."))
            : MapErrorToResponse(result.Error);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<AuthResponse>.Ok(result.Value))
            : Unauthorized(ApiResponse.Fail(result.Error.Description));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<AuthResponse>.Ok(result.Value))
            : Unauthorized(ApiResponse.Fail(result.Error.Description));
    }

    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LogoutCommand(_currentUser.UserId), cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Logged out successfully."))
            : MapErrorToResponse(result.Error);
    }

    private IActionResult MapErrorToResponse(Error error) => error.Code switch
    {
        "Error.NotFound" => NotFound(ApiResponse.Fail(error.Description)),
        "Error.Forbidden" => Forbid(),
        "Error.Conflict" => Conflict(ApiResponse.Fail(error.Description)),
        _ => BadRequest(ApiResponse.Fail(error.Description))
    };
}
