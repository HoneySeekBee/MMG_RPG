using System.Net.Http;
using Newtonsoft.Json;
using System.Numerics;
using GameServer.Game.Room;
using GameServer.Game.Object;
using System.Text;

namespace GameServer.Data
{
    public class API
    {
        private readonly HttpClient _httpClient;

        public API()
        {
            _httpClient = new HttpClient(); // 여기서 초기화
        }
        public async Task<CharacterObject> GetCharacterStatus(int characterId, string name, GameRoom room)
        {
            string url = Program.URL + $"/api/character/status/{characterId}";

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
        public async Task<bool> UpdateCharacterStatus(UpdateCharacterStatusRequest request)
        {
            string url = Program.URL + "/api/character/status/update";
            try
            {
                string json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[API 실패] 상태 업데이트 실패: {response.StatusCode}");
                    return false;
                }

                Console.WriteLine("[API] 캐릭터 상태 업데이트 성공");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API 예외] 상태 업데이트 중 오류 발생: {ex.Message}");
                return false;
            }
        }
        public class UpdateCharacterStatusRequest
        {
            public int CharacterId { get; set; }
            public int? CharacterLevel { get; set; }
            public float? HP { get; set; }
            public float? MP { get; set; }
            public float? NowHP { get; set; }
            public float? NowMP { get; set; }
            public float? Exp { get; set; }
            public float? MaxExp { get; set; }
            public int? Gold { get; set; }
        }
    }
}
