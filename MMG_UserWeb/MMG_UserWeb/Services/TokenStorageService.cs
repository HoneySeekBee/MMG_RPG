using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace MMG_UserWeb.Services
{
    public class TokenStorageService
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private const string TokenKey = "authToken";

        public TokenStorageService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task SaveTokenAsync(string token)
        {
            await _sessionStorage.SetAsync(TokenKey, token);
            Console.WriteLine($"토큰 저장 완료 {token}");
        }

        public async Task<string?> GetTokenAsync()
        {
            var result = await _sessionStorage.GetAsync<string>(TokenKey);
            return result.Success ? result.Value : null;
        }

        public async Task RemoveTokenAsync()
        {
            await _sessionStorage.DeleteAsync(TokenKey);
        }
    }
}
