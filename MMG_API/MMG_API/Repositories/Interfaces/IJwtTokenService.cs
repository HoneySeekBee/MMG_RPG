using MMG_API.Models;
using System.Security.Claims;

namespace MMG_API.Repositories.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        public ClaimsPrincipal ValidateToken(string token, out string reason);
    }
}
