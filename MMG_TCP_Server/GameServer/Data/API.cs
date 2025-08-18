using System.Net.Http;
using Newtonsoft.Json;
using System.Numerics;
using GameServer.Game.Room;
using GameServer.Game.Object;
using System.Text;
using System.Xml.Linq;
using System.Text.Json;
using QuestPacket;
using static GameServer.Game.Quest.CharacterQuest;
using GameServer.Game.Quest;
using System.Text.Json.Serialization;

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

                await _character.GetCharacterQuest();

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
        public class UserQuestDto
        {
            public int CharacterId { get; set; }
            public int QuestId { get; set; }
            public QuestProgressStatus Status { get; set; }
            public string Progress { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
        public async Task<List<UserQuestDto>> GetUserQuest(int characterId)
        {
            string url = Program.URL + $"/api/UserQuest/{characterId}";
            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[API 실패] GetUserQuest 상태 코드: {response.StatusCode}");
                    return null;
                }

                string result = await response.Content.ReadAsStringAsync();

                // 가지고 오기 
                var dtoList = System.Text.Json.JsonSerializer.Deserialize<List<UserQuestDto>>(result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (dtoList == null)
                {
                    Console.WriteLine("[API] DTO 역직렬화 실패");
                    return null;
                }

                return dtoList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Exception] GetUserQuest " + ex);
                return null;
            }
        }
        public async Task<bool> ReportProgressBatchAsync(int characterId, ReportQuestProgressBatchRequest req, CancellationToken ct = default)
        {
            string url = $"{Program.URL}/api/UserQuest/{characterId}/progress-batch";
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(req, jsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await _httpClient.PostAsync(url, content, ct);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[API 실패] 유저 퀘스트 업데이트 실패: {response.StatusCode}");
                    return false;
                }

                Console.WriteLine("[API] 유저퀘스트 업데이트 성공");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API 예외] 유저 퀘스트 업데이트 중 오류 발생: {ex.Message}");
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
        public sealed class ReportQuestProgressBatchRequest
        {
            // 멱등성을 위해 배치 ID(서버에서 중복 처리 방지)
            public string BatchId { get; set; } = Guid.NewGuid().ToString();

            public List<Item> Items { get; set; } = new();

            public sealed class Item
            {
                public int QuestId { get; set; }

                // "kill:101" / "collect:5002" -> +증가량
                public Dictionary<string, int> ProgressDelta { get; set; } = new(StringComparer.Ordinal);
            }
        }
    }
}
