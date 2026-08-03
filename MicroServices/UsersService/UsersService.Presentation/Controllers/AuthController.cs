using EventManagement.Contracts.Api;
using EventManagement.Contracts.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersService.Application.Abstractions.Services;

namespace UsersService.Presentation.Controllers;

/// <summary>
/// 
/// </summary>
/// <param name="userService"></param>
[Route("[controller]")]
[ApiController]
[Authorize]
public class AuthController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <param name="registerUserRequest"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("register")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status400BadRequest)]
    public async Task<NoContentResult> RegisterAsync([FromBody] RegisterUserRequest registerUserRequest, CancellationToken ct = default)
    {
        await userService.RegisterAsync(registerUserRequest.Login, registerUserRequest.Password, registerUserRequest.Role, ct);

        return NoContent();
    }

    /// <summary>
    /// Логин
    /// </summary>
    /// <param name="loginUserRequest"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status400BadRequest)]
    public async Task<OkObjectResult> LoginAsync([FromBody] LoginUserRequest loginUserRequest, CancellationToken ct = default)
    {
        var token = await userService.LoginAsync(loginUserRequest.Login, loginUserRequest.Password, ct);

        return Ok(new { Token = token });
    }
}
