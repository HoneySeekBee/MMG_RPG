using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMG_API.Data;
using MMG_API.DTOs;
using MMG_API.Models;
using System.Text.Json;

namespace MMG_API.Controllers.Quest
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserQuestController : ControllerBase
    {
        private readonly MMGDbContext _db;
        public UserQuestController(MMGDbContext db)
        {
            _db = db;
        }

        // [1] Read
        [HttpGet("{characterId}")]
        public async Task<IActionResult> Get(int characterId)
        {
            var goals = await _db.UserQuest
                .Where(g => g.CharacterId == characterId)
                .Select(g => new UserQuestDto
                {
                    CharacterId = g.CharacterId,
                    QuestId = g.QuestId,
                    Status = g.Status,
                    Progress = g.Progress,
                    StartedAt = g.StartedAt,
                    CompletedAt = g.CompletedAt,
                    UpdatedAt = g.UpdatedAt,
                })
                .ToListAsync();

            return Ok(goals);
        }

        // [2] Create
        [HttpPost("{characterId:int}/accept")]
        public async Task<ActionResult<UserQuestDto>> Accept(int characterId, [FromBody] AcceptQuestRequest req, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;

            // (1) 퀘스트 Id가 유효한지 확인하기
            var quest = await _db.Quests.AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuestId == req.QuestId, ct);

            if (quest is null)
                return NotFound($"[UserQuestController] {req.QuestId} not found");

            // (2) 기존 상태 조회 (중복 수락 등)
            var existing = await _db.UserQuest.FindAsync(new object[] { characterId, req.QuestId }, ct);
            if (existing != null)
            {
                if (existing.Status == (byte)QuestStatus.ACTIVE)
                    return Conflict("Quest already active");
                if (existing.Status == (byte)QuestStatus.COMPLETED)
                    return Conflict("Quest already completed");
            }

            // (3) 시작 가능 재검증 
            if (await CheckCanStartQuest(characterId, req.QuestId, ct) == false)
            {
                return Conflict("Quest Failed Accept");
            }

            // (4) 생성 또는 재수락 갱신 
            var entity = existing ?? new UserQuestEntity
            {
                CharacterId = characterId,
                QuestId = req.QuestId,
            };
            entity.Status = (byte)QuestStatus.ACTIVE;
            entity.Progress = null;
            entity.StartedAt = entity.StartedAt == default ? now : entity.StartedAt;
            entity.CompletedAt = null;
            entity.UpdatedAt = now;

            if (existing == null) _db.UserQuest.Add(entity);

            await _db.SaveChangesAsync();

            // (5) 응답 DTO 
            var dto = new UserQuestDto
            {
                CharacterId = entity.CharacterId,
                QuestId = entity.QuestId,
                Status = entity.Status,
                Progress = entity.Progress,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                UpdatedAt = entity.UpdatedAt,
            };
            return CreatedAtAction(nameof(Get), new { characterId }, dto);
        }
        private async Task<bool> CheckCanStartQuest(int characterId, int questId, CancellationToken ct)
        {
            // [1] 시작 가능 레벨에 부합한지?

            // 시작 가능 레벨 불러오기 
            var minLevel = await _db.Quests.AsNoTracking()
                .Where(q => q.QuestId == questId)
                .Select(q => q.MinLevel)
                .SingleOrDefaultAsync(ct);

            // 유저 레벨 가지고 오기 
            var userLevel = await _db.CharacterStatuses.AsNoTracking()
                .Where(q => q.CharacterId == characterId)
                .Select(q => q.CharacterLevel)
                .SingleAsync(ct);

            if (userLevel < minLevel)
                return false;

            // [2] 선행 퀘스트를 완료 했는지?

            // 선행 퀘스트 목록 가지고 오기 
            var prereqIds = await _db.Quests.AsNoTracking()
                .Where(q => q.QuestId == questId)
                .Select(q => q.PrevQuestIds)
                .SingleOrDefaultAsync(ct);
            List<int> preQuestIdList = ParsePrereqIds(prereqIds);
            if (preQuestIdList.Count == 0) return true;

            var completedIds = await _db.UserQuest.AsNoTracking()
                .Where(uq => uq.CharacterId == characterId
                && uq.Status == (byte)QuestStatus.COMPLETED
                && preQuestIdList.Contains(uq.QuestId))
                .Select(up => up.QuestId)
                .Distinct()
                .ToListAsync(ct);

            return preQuestIdList.All(id => completedIds.Contains(id));
        }
        private static List<int> ParsePrereqIds(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return new();

            s = s.Trim();

            // 값이 "1,3,4"처럼 양쪽에 따옴표까지 감싸져 온다면 제거
            if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
                s = s.Trim('"');

            // JSON 배열 형태라면 JSON으로 파싱
            if (s.StartsWith("["))
            {
                try
                {
                    using var doc = JsonDocument.Parse(s);
                    var list = new List<int>();
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var n))
                            list.Add(n);
                        else if (el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out var n2))
                            list.Add(n2);
                    }
                    return list.Distinct().ToList();
                }
                catch
                {
                    // JSON 파싱 실패 시 아래 CSV 로직으로 폴백
                }
            }

            // "1,3,4" 같은 CSV 처리
            var ids = new List<int>();
            foreach (var part in s.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var t = part.Trim().Trim('"', '\'');
                if (int.TryParse(t, out var n)) ids.Add(n);
            }
            return ids.Distinct().ToList();
        }

        // [3] Update

        [HttpPost("{characterId:int}/progress/{questId:int}")]
        public async Task<ActionResult<UserQuestDto>> ReportProgress(int characterId, int questId, [FromBody] ReportQuestProgressRequest req, CancellationToken ct)
        {
            var entity = await _db.UserQuest.FirstOrDefaultAsync(x => x.CharacterId == characterId && x.QuestId == questId, ct);

            if (entity is null) return NotFound("Quest not accepted.");

            if (entity.Status != (byte)QuestStatus.ACTIVE) return Conflict("Quest is not Active");

            var cur = ParseProgress(entity.Progress);

            foreach (var (k, v) in req.ProgressDelta)
            {
                cur.TryGetValue(k, out var old);
                var next = old + v;
                cur[k] = next < 0 ? 0 : next;
            }
            entity.Progress = System.Text.Json.JsonSerializer.Serialize(cur);
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);

            return Ok(new UserQuestDto
            {
                CharacterId = entity.CharacterId,
                QuestId = entity.QuestId,
                Status = entity.Status,
                Progress = entity.Progress,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                UpdatedAt = entity.UpdatedAt
            });
        }

        [HttpPost("{characterId:int}/complete/{questId:int}")]
        public async Task<ActionResult<UserQuestDto>> Complete(int characterId, int questId, CancellationToken ct)
        {
            // [1] 현재 퀘스트 조회 
            var entity = await _db.UserQuest
                .FirstOrDefaultAsync(x => x.CharacterId == characterId && x.QuestId == questId, ct);

            if (entity is null)
                return NotFound("Quest not accepted");

            if (entity.Status != (byte)QuestStatus.ACTIVE)
                return Conflict("Qeust is not active.");

            // [2] 목표 충족 여부 서버 검증 
            var progress = ParseProgress(entity.Progress);
            var ok = await CheckAchevGoal(characterId, questId, progress, ct);
            if (!ok) return BadRequest("Objectives not met.");

            // [3] 상태 전이 + 타임스태프
            var now = DateTimeOffset.UtcNow;
            entity.Status = (byte)QuestStatus.COMPLETED;
            entity.CompletedAt = now;
            entity.UpdatedAt = now;

            // 보상 보내주기 

            await _db.SaveChangesAsync(ct);
            return Ok(new UserQuestDto
            {
                CharacterId = entity.CharacterId,
                QuestId = entity.QuestId,
                Status = entity.Status,
                Progress = entity.Progress,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                UpdatedAt = entity.UpdatedAt
            });
        }

        private async Task<bool> CheckAchevGoal(int characterId, int questId, Dictionary<string, int> progress, CancellationToken ct)
        {
            // [1] 퀘스트 목표 가지고 오기
            var goals = await _db.QuestGoals
                .AsNoTracking()
                .Where(g => g.QuestId == questId)
                .Select(g => new { g.GoalType, g.TargetId, g.Count })
                .ToListAsync(ct);

            if (goals.Count == 0) return true; // 특정 NPC에게 말걸기 

            var requires = goals
                .GroupBy(g => new { g.GoalType, g.TargetId, })
                .Select(grp => new
                {
                    grp.Key.GoalType,
                    grp.Key.TargetId,
                    Required = grp.Sum(x => x.Count)
                })
                .ToList();

            // [2] 아이템 목표는 나중에 인벤토리 구현되면 체크하자. 
            var needItemIds = requires.Where(r => r.GoalType == 1).Select(r => r.TargetId).Distinct().ToList();
            // 여기에 유저 인벤토리에 있는거 체크 

            // [3] 각 요구사항 충족 여부 검사하기 
            foreach(var need in requires)
            {
                switch (need.GoalType)
                {
                    case 0:
                        {
                            var key = $"kill:{need.TargetId}";
                            var have = progress.TryGetValue(key, out var v) ? v : 0;
                            if (have < need.Required) return false;
                            break;
                        }
                    case 1:
                        {
                            var key = $"collect:{need.TargetId}";
                            var viaProgress = progress.TryGetValue(key, out var v) ? v : 0;
                            //var viaInventory = inventory.TryGetValue(need.TargetId, out var cnt) ? cnt : 0;

                            // 진행도 보고(or 서버 인벤) 중 더 신뢰 가능한 값을 사용
                            //var have = Math.Max(viaProgress, viaInventory);
                            //if (have < need.Required) return false;
                            break;
                        }
                    default:
                        return false;
                }
            }

            return true;
        }

        // [4] Delete

        [HttpPost("{characterId:int}/abandon/{questId:int}")]
        public async Task<ActionResult<UserQuestDto>> Abandon(int characterId, int questId, [FromBody] AbandonQuestRequest? req, CancellationToken ct)
        {
            // [1] 퀘스트 조회하기 
            var entity = await _db.UserQuest
                .FirstOrDefaultAsync(x => x.CharacterId == characterId && x.QuestId == questId, ct);

            if (entity is null)
                return NotFound("Quest not accepted");

            // [2] 퀘스트 포기 가능 조건? 상태가 진행 중인 것 
            if (entity.Status != (byte)QuestStatus.ACTIVE)
                return Conflict("Quest is not active");

            // [3] 상태를 포기로 바꾼다. 
            var now = DateTimeOffset.UtcNow;
            entity.Status = (byte)QuestStatus.ABANDONED;
            entity.UpdatedAt = now;
            entity.CompletedAt = null;

            // [4] 진행도 처리 : 일단 대부분의 퀘스트는 진행도 포기 
            var keep = req?.KeepProgress ?? false;
            if (!keep) entity.Progress = null;

            await _db.SaveChangesAsync(ct);

            return Ok(new UserQuestDto
            {
                CharacterId = entity.CharacterId,
                QuestId = entity.QuestId,
                Status = entity.Status,
                Progress = entity.Progress,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                UpdatedAt = entity.UpdatedAt
            });
        }

        private static Dictionary<string, int> ParseProgress(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new();
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(json)
                       ?? new Dictionary<string, int>();
            }
            catch { return new(); }
        }
    }
    public enum QuestStatus
    {
        ACTIVE = 1,
        COMPLETED = 2,
        FAILED = 3,
        ABANDONED = 4,
    }
    public sealed class AcceptQuestRequest
    {
        public int QuestId { get; set; }
    }
    public sealed class ReportQuestProgressRequest
    {
        // 누적이 아니라 "증가분"을 추천 (경쟁조건에 강함)
        public Dictionary<string, int> ProgressDelta { get; set; } = new();
    }
    public sealed class AbandonQuestRequest
    {
        public bool KeepProgress { get; set; } = false;
    }
    public class UserQuestDto
    {
        public int CharacterId { get; set; }
        public int QuestId { get; set; }
        public byte Status { get; set; }
        public string? Progress { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
