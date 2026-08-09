using EventManagement.Domain.Entities;

namespace EventManagement.Application.Abstractions.Security;

public interface IJwtService
{
    public string GenerateToken(User user);
}
