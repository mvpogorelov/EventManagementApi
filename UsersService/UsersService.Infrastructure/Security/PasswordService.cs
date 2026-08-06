using System.Security.Cryptography;
using System.Text;
using UsersService.Application.Abstractions.Security;

namespace UsersService.Infrastructure.Security;

public class PasswordService : IPasswordService
{
    public string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

        return Convert.ToHexString(bytes);
    }

    public bool Verify(string password, string passwordHash) => Hash(password) == passwordHash;
}
