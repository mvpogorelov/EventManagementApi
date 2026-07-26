using EventManagement.Domain.Common;
using EventManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventManagement.Presentation.Controllers;

[Authorize]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentUserId => Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
        ? id
        : throw new UnAuthenticatedException("Пользователь не определён");
    
    protected UserRole CurrentUserRole => Enum.TryParse(User.FindFirst(ClaimTypes.Role)?.Value, out UserRole role)
        ? role
        : throw new UnAuthenticatedException("Пользователь не определён");
}
