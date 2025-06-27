using MMG_API.Models;

namespace MMG_API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        string GenerateToken(User user);
    }
}
