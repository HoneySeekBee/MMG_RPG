using MMG_API.Models;

namespace MMG_API.Repositories.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
