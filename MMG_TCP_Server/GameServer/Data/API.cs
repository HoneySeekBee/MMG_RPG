using System.Net.Http;
using Newtonsoft.Json;
using System.Numerics;
using GameServer.Game.Room;
using GameServer.Game.Object;

namespace GameServer.Data
{
    public class API
    {
        private readonly HttpClient _httpClient;

        public static string MMG_API_URL = "http://localhost:5070";
        public API()
        {
            _httpClient = new HttpClient(); // 여기서 초기화
        }
        public async Task<CharacterObject> GetCharacterStatus(int characterId, string name, GameRoom room)
        {
            string url = API.MMG_API_URL + $"/api/character/status/{characterId}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[API 실패] 상태 코드: {response.StatusCode}");
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();
                var dto = JsonConvert.DeserializeObject<CharacterStatusDTO>(json);

                if (dto == null)
                {
                    Console.WriteLine("[API] DTO 역직렬화 실패");
                    return null;
                }

                var _character = CharacterObject.ConvertToRuntimeStatus(dto, name, room);
                return _character;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API 예외] {ex.Message}");
                return null;
            }
        }
    }
}
