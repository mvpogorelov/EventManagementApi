using UsersService.Domain.Entities;

namespace UsersService.Application.Abstractions.Security;

public interface IJwtService
{
    public string GenerateToken(User user);
}
