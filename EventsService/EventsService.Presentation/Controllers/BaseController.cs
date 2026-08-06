using EventManagement.Contracts.Common;
using EventsService.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventsService.Presentation.Controllers;

[Authorize]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentUserId => Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
        ? id
        : throw new UnauthorizedException("Пользователь не определён");

    protected UserRole CurrentUserRole => Enum.TryParse(User.FindFirst(ClaimTypes.Role)?.Value, out UserRole role)
        ? role
        : throw new UnauthorizedException("Пользователь не определён");
}
