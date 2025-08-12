using Microsoft.AspNetCore.Mvc;
using MMG_AdminTool.Models;
using System.Text.Json;

namespace MMG_AdminTool.Controllers
{
    public class QuestCreatorController : Controller
    {
        private readonly HttpClient _httpClient;

        public QuestCreatorController(IHttpClientFactory httpFactory)
        {
            _httpClient = httpFactory.CreateClient("API");
        }
        #region CRUD 
        // [1] Read : 퀘스트 목록 조회  
        public async Task<IActionResult> Index()
        {
            var quests = await GetQuestList();
            return View(quests);
        }

        #endregion
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var quests = await GetQuestList(); // [1] 퀘스트 목록 받아오기 
            var npcs = await GetNpcList(); // [2] NPC 목록 요청
            var items = await GetItemList(); // [3] Item 목록 요청 
            var monsters = await GetMonsterList();  // [4] Monster 가지고 오기

            var vm = new QuestViewModel
            {
                SelectedPrevQuests = new(), // null 방지
                AllQuests = quests.Select(q => new QuestSummaryViewModel
                {
                    QuestId = q.QuestId,
                    Title = q.Title,
                    MinLevel = q.MinLevel
                }).ToList(),

                AllNpcs = npcs.Select(q => new NpcSummaryViewModel
                {
                    NpcId = q.TemplateId,
                    Name = q.Name,
                }).ToList(),

                AllItems = items.Select(item => new ItemSummary
                {
                    ItemId = item.ItemId,
                    Name = item.Name,
                }).ToList(),

                AllMonsters = monsters
                .Select(monster => new MonsterSummary
                {
                    MonsterId = monster.Id,
                    Name = monster.Name,
                }).ToList()
            };

            return View(vm);
        }
        public class NpcSummaryViewModel
        {
            public int NpcId { get; set; }
            public string Name { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> Create(QuestViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var ok = await SaveAllAsync(model, isEdit: false, ct);
            if (!ok)
                return View(model); // SaveAllAsync에서 ModelState 에러 메시지 이미 채웠음

            TempData["Toast"] = "퀘스트가 저장되었습니다.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
        {
            try
            {
                Console.WriteLine("[Edit] start");

                var questRes = await _httpClient.GetAsync($"/api/quest/{id}", ct);
                Console.WriteLine($"[Edit] questRes {(int)questRes.StatusCode}");
                if (!questRes.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

                var quest = await questRes.Content.ReadFromJsonAsync<QuestViewModel>(cancellationToken: ct);
                if (quest is null) return RedirectToAction(nameof(Index));

                Console.WriteLine("[Edit] before GetQuestList");
                var allQuests = await GetQuestList(ct);
                Console.WriteLine("[Edit] after GetQuestList");

                Console.WriteLine("[Edit] before GetNpcList");
                var allNpcs = await GetNpcList(ct);
                Console.WriteLine("[Edit] after GetNpcList");

                Console.WriteLine("[Edit] before GetItemList");
                var allItems = await GetItemList(ct);
                Console.WriteLine("[Edit] after GetItemList");

                Console.WriteLine("[Edit] before GetMonsterList");
                var allMonsters = await GetMonsterList(ct);
                Console.WriteLine("[Edit] after GetMonsterList");

                Console.WriteLine("[Edit] before goals");
                var goalsMsg = await _httpClient.GetAsync($"/api/questGoal/{id}", ct);
                Console.WriteLine($"[Edit] goals {(int)goalsMsg.StatusCode}");

                quest.QuestGoals = goalsMsg.IsSuccessStatusCode
                    ? (await goalsMsg.Content.ReadFromJsonAsync<List<QuestGoalDto>>(cancellationToken: ct) ?? new())
                    : new List<QuestGoalDto>();

                var prevIds = ParsePrevQuestIds(quest.PrevQuestIds);

                quest.AllQuests = allQuests
                    .Where(q => q.QuestId != id)
                    .Select(q => new QuestSummaryViewModel { QuestId = q.QuestId, Title = q.Title, MinLevel = q.MinLevel })
                    .ToList();

                quest.SelectedPrevQuests = quest.AllQuests.Where(q => prevIds.Contains(q.QuestId)).ToList();
                quest.AllNpcs = allNpcs.Select(n => new NpcSummaryViewModel { NpcId = n.TemplateId, Name = n.Name }).ToList();
                quest.AllItems = allItems.Select(i => new ItemSummary { ItemId = i.ItemId, Name = i.Name }).ToList();
                quest.AllMonsters = allMonsters.Select(m => new MonsterSummary { MonsterId = m.Id, Name = m.Name }).ToList();

                Console.WriteLine("[Edit] done");
                return View(quest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Edit] ERROR: {ex.GetType().Name} - {ex.Message}");
                throw; // 또는 ModelState에 넣고 View로
            }
        }
        private static List<int> ParsePrevQuestIds(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new();

            // JSON 배열("[1,2,3]") 우선
            try
            {
                var list = System.Text.Json.JsonSerializer.Deserialize<List<int>>(raw);
                if (list != null) return list;
            }
            catch { /* JSON 아니면 폴백 */ }

            // CSV("1,2,3") 폴백
            try
            {
                return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
                          .Where(v => v.HasValue).Select(v => v!.Value)
                          .ToList();
            }
            catch { return new(); }
        }
        // [5] 퀘스트 수정 - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuestViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.QuestId = id; // 보장
            var ok = await SaveAllAsync(model, isEdit: true, ct);
            if (!ok)
                return View(model);

            TempData["Toast"] = "퀘스트가 수정되었습니다.";
            return RedirectToAction(nameof(Index));
        }
        // [6] 퀘스트 삭제
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync($"/api/quest/{id}");
            return RedirectToAction("Index");
        }
        #region Upsert 메소드 : 있으면 update 없으면 Create
        private async Task<bool> SaveAllAsync(QuestViewModel model, bool isEdit, CancellationToken ct)
        {
            // 1) 사전 보정
            if (model.SortOrder == 0) model.SortOrder = model.MinLevel;
            if (model.MinLevel < 1 || model.MinLevel > 200) model.MinLevel = 1;

            // 2) Upsert: Quest
            var questId = await UpsertQuestAsync(model, isEdit, ct);
            if (questId == 0) { ModelState.AddModelError("", "퀘스트 저장 실패"); return false; }

            // 3) Upsert: NPC 링크 (있으면 갱신/없으면 생성, 없애고 싶다면 API에서 교체형으로)
            if (model.StartTriggerType == 1)
            {
                var linkOk = await UpsertNpcLinksAsync(questId, isEdit, 0, model.StartNpcId ?? -1, ct);
                if (!linkOk) { ModelState.AddModelError("", "NPC 링크 저장 실패"); return false; }
            }
            if (model.EndTriggerType == 1)
            {
                var linkOk = await UpsertNpcLinksAsync(questId, isEdit, 1, model.EndNpcId ?? -1, ct);
                if (!linkOk) { ModelState.AddModelError("", "NPC 링크 저장 실패"); return false; }
            }
            // 4) Upsert: Goals (교체형 PUT 권장)
            var goalsOk = await UpsertGoalsAsync(questId, model, ct);
            if (!goalsOk) { ModelState.AddModelError("", "목표 저장 실패"); return false; }

            // 5) Upsert: Reward (quest당 1개 → PUT /questReward/{questId} 권장)
            var rewardOk = await UpsertRewardAsync(questId, model, isEdit, ct);
            if (!rewardOk) { ModelState.AddModelError("", "보상 저장 실패"); return false; }

            return true;
        }
        private async Task<int> UpsertQuestAsync(QuestViewModel model, bool isEdit, CancellationToken ct = default)
        {
            var apiModel = new
            {
                model.QuestId,
                model.Title,
                model.Description,
                model.IconCode,
                model.Type,
                model.SortOrder,
                model.MinLevel,
                PrevQuestIds = (model.PrevQuestIds?.Any() == true)
                    ? JsonSerializer.Serialize(model.PrevQuestIds)
                    : null,
                model.StartTriggerType,
                model.StartNpcId,
                model.EndTriggerType,
                model.EndNpcId
            };

            HttpResponseMessage res;

            if (isEdit)
            {
                res = await _httpClient.PutAsJsonAsync($"/api/quest/{model.QuestId}", apiModel, ct);
                if (!res.IsSuccessStatusCode) return 0;
                return model.QuestId;
            }
            else
            {
                res = await _httpClient.PostAsJsonAsync("/api/quest", apiModel, ct);
                if (!res.IsSuccessStatusCode) return 0;

                var created = await res.Content.ReadFromJsonAsync<QuestViewModel>(cancellationToken: ct);
                return created?.QuestId ?? 0;
            }
        }
        private async Task<bool> UpsertNpcLinksAsync(int questId, bool isEdit, int TriggerType, int NpcId, CancellationToken ct)
        {
            if (NpcId == -1)
            {
                Console.WriteLine($"NpcLinkAsync {TriggerType}에서 NpcId가 -1입니다.");
                return false;
            }
            var payload = new
            {
                NpcTemplateId = NpcId,
                QuestId = questId,
                LinkType = TriggerType,
            };
            Console.WriteLine($"NpcLink NPC ID : {NpcId} / TriggerType {TriggerType}");
            if (isEdit)
            {
                var res = await _httpClient.PutAsJsonAsync($"/api/NpcQuestLink/{questId}/{TriggerType}", payload, ct);
                return res.IsSuccessStatusCode;
            }
            else
            {
                var res = await _httpClient.PostAsJsonAsync($"/api/NpcQuestLink", payload, ct);
                return res.IsSuccessStatusCode;
            }
        }
        private async Task<bool> UpsertGoalsAsync(int questId, QuestViewModel model, CancellationToken ct = default)
        {
            var payloads = (model.QuestGoals?.Any() == true) ? model.QuestGoals.Select((g, i) => new QuestGoalDto
            {
                QuestId = questId,
                GoalIndex = i,
                GoalType = g.GoalType,
                TargetId = g.TargetId,
                Count = g.Count
            })
            .OrderBy(x => x.GoalIndex).ToList() : new List<QuestGoalDto>();

            var res = await _httpClient.PutAsJsonAsync($"/api/questGoal/{questId}/goals", payloads, ct);
            return res.IsSuccessStatusCode;
        }
        private async Task<bool> UpsertRewardAsync(int questId, QuestViewModel model, bool isEdit, CancellationToken ct = default)
        {
            var dto = new QuestRewardDto
            {
                QuestId = questId,
                Exp = model.EXP,
                JsonReward = (model.Rewards?.Any() == true)
                    ? JsonSerializer.Serialize(model.Rewards)
                    : null
            };

            // PUT upsert 엔드포인트가 있으면 한 줄

            if (isEdit)
            {
                var res = await _httpClient.PutAsJsonAsync($"/api/QuestReward/{questId}", dto, ct);
                return res.IsSuccessStatusCode;
            }
            else
            {
                var res = await _httpClient.PostAsJsonAsync($"/api/QuestReward", dto, ct);
                return res.IsSuccessStatusCode;
            }
        }
        #endregion
        #region 사용 데이터 Read
        public async Task<List<QuestViewModel>> GetQuestList(CancellationToken ct = default)
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<QuestViewModel>>("/api/quest", ct);
                var list = data ?? new List<QuestViewModel>();
                Console.WriteLine($"[GetQuestList] ok count={list.Count}"); // WriteLine로 줄바꿈
                return list;
            }
            catch (OperationCanceledException oce)
            {
                Console.WriteLine($"[GetQuestList] CANCELED: {oce.Message}");
                return new List<QuestViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetQuestList] ERROR: {ex.GetType().Name} - {ex.Message}");
                return new List<QuestViewModel>();
            }
        }
        public async Task<List<NpcTemplateViewModel>> GetNpcList(CancellationToken ct = default)
        {
            try
            {
                var npcResponse = await _httpClient.GetAsync("/api/npctemplate", ct);
                var npcs = await npcResponse.Content.ReadFromJsonAsync<List<NpcTemplateViewModel>>();
                Console.Write($"[GetNpcList] - Npc count {npcs.Count}");
                return npcs ?? new List<NpcTemplateViewModel>();
            }
            catch (HttpRequestException ex)
            {
                return new List<NpcTemplateViewModel>();
            }
        }
        public async Task<List<ItemViewModel>> GetItemList(CancellationToken ct = default)
        {
            try
            {
                var itemResponse = await _httpClient.GetAsync("/api/items");
                var items = await itemResponse.Content.ReadFromJsonAsync<List<ItemViewModel>>();
                Console.Write($"[GetItemList] - item count {items.Count}");
                return items ?? new List<ItemViewModel>();
            }
            catch (HttpRequestException ex)
            {
                return new List<ItemViewModel>();
            }
        }
        public async Task<List<MonsterModel>> GetMonsterList(CancellationToken ct = default)
        {
            try
            {
                var monsterResponse = await _httpClient.GetAsync("/api/monster/all");
                var monsters = await monsterResponse.Content.ReadFromJsonAsync<List<MonsterModel>>();
                Console.Write($"[GetMonsterList] - monsters count {monsters.Count}");
                return monsters ?? new List<MonsterModel>();
            }
            catch (HttpRequestException ex)
            {
                return new List<MonsterModel>();
            }
        }

        #endregion
    }
}
