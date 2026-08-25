using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostForge.Application.Auth.Commands.Login;
using PostForge.Application.Auth.Commands.Logout;
using PostForge.Application.Auth.Commands.RefreshToken;
using PostForge.Application.Auth.DTOs;
using PostForge.Application.Auth.Queries.GetCurrentUser;

namespace PostForge.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginCommand command)
    {
        var result = await mediator.Send(command);
        if (result is null)
            return Unauthorized();

        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResultDto>> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await mediator.Send(command);
        if (result is null)
            return Unauthorized();

        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        var revoked = await mediator.Send(command);
        if (!revoked)
            return BadRequest(new { message = "Invalid or already revoked refresh token." });

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var currentUser = await mediator.Send(new GetCurrentUserQuery());
        if (currentUser is null)
            return Unauthorized();

        return Ok(currentUser);
    }
}