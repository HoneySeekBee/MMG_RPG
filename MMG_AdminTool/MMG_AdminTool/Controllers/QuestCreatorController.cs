using Microsoft.AspNetCore.Mvc;
using MMG_AdminTool.Models;
using Newtonsoft.Json;

namespace MMG_AdminTool.Controllers
{
    public class QuestCreatorController : Controller
    {
        private readonly HttpClient _httpClient;

        public QuestCreatorController(IHttpClientFactory httpFactory)
        {
            _httpClient = httpFactory.CreateClient("API");
        }

        // [1] 퀘스트 목록 조회
        public async Task<IActionResult> Index()
        {
            var quests = await _httpClient.GetFromJsonAsync<List<QuestViewModel>>("/api/quest");
            return View(quests);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // [1] 퀘스트 목록 요청
            var response = await _httpClient.GetAsync("/api/quest");
            if (!response.IsSuccessStatusCode)
            {
                // 실패 처리
                return View(new QuestViewModel());
            }
            var quests = await response.Content.ReadFromJsonAsync<List<QuestViewModel>>();

            // [2] NPC 목록 요청
            var npcResponse = await _httpClient.GetAsync("/api/npctemplate");
            if (!npcResponse.IsSuccessStatusCode)
            {
                return View(new QuestViewModel());
            }
            var npcs = await npcResponse.Content.ReadFromJsonAsync<List<NpcTemplateViewModel>>();

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
        public async Task<IActionResult> Create(QuestViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // [1] SortOrder가 0이면 MinLevel로 설정
            if (model.SortOrder == 0)
                model.SortOrder = model.MinLevel;

            // [2] MinLevel 유효성 강제 보정
            if (model.MinLevel < 1 || model.MinLevel > 200)
                model.MinLevel = 1;

            // [3] PrevQuestIds를 JSON 문자열로 직렬화해서 전송용 모델 구성
            var apiModel = new
            {
                model.QuestId,
                model.Title,
                model.Description,
                model.IconCode,
                model.Type,
                model.SortOrder,
                model.MinLevel,
                PrevQuestIds = model.PrevQuestIds != null && model.PrevQuestIds.Any()
                    ? JsonConvert.SerializeObject(model.PrevQuestIds)
                    : null,
                model.StartTriggerType,
                model.StartNpcId,
                model.EndTriggerType,
                model.EndNpcId
            };

            var response = await _httpClient.PostAsJsonAsync("/api/quest", apiModel);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "퀘스트 생성 실패");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // [4] 퀘스트 수정 - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"/api/quest/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var quest = await response.Content.ReadFromJsonAsync<QuestViewModel>();

            // [1] 전체 퀘스트 목록 가져오기 (선택용)
            var allQuests = await _httpClient.GetFromJsonAsync<List<QuestViewModel>>("/api/quest");

            // [2] 전체 NPC 목록 가져오기
            var allNpcs = await _httpClient.GetFromJsonAsync<List<NpcTemplateViewModel>>("/api/npctemplate");

            // [3] PrevQuestIds 문자열 → List<int> 변환
            var prevQuestIds = quest.PrevQuestIds?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList() ?? new();

            quest.SelectedPrevQuests = allQuests
                .Where(q => prevQuestIds.Contains(q.QuestId))
                .Select(q => new QuestSummaryViewModel
                {
                    QuestId = q.QuestId,
                    Title = q.Title,
                    MinLevel = q.MinLevel
                }).ToList();

            quest.AllQuests = allQuests
                .Select(q => new QuestSummaryViewModel
                {
                    QuestId = q.QuestId,
                    Title = q.Title,
                    MinLevel = q.MinLevel
                }).ToList();

            quest.AllNpcs = allNpcs
                .Select(npc => new NpcSummaryViewModel
                {
                    NpcId = npc.TemplateId,
                    Name = npc.Name
                }).ToList();

            return View(quest);
        }

        // [5] 퀘스트 수정 - POST
        [HttpPost]
        public async Task<IActionResult> Edit(QuestViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // [1] SortOrder가 0이면 MinLevel로 설정
            if (model.SortOrder == 0)
                model.SortOrder = model.MinLevel;

            // [2] MinLevel 유효성 보정
            if (model.MinLevel < 1 || model.MinLevel > 200)
                model.MinLevel = 1;

            // [3] PrevQuestIds를 JSON 문자열로 직렬화해서 전송
            var apiModel = new
            {
                model.QuestId,
                model.Title,
                model.Description,
                model.IconCode,
                model.Type,
                model.SortOrder,
                model.MinLevel,
                PrevQuestIds = model.PrevQuestIds != null && model.PrevQuestIds.Any()
                    ? JsonConvert.SerializeObject(model.PrevQuestIds)
                    : null,
                model.StartTriggerType,
                model.StartNpcId,
                model.EndTriggerType,
                model.EndNpcId
            };

            var response = await _httpClient.PutAsJsonAsync($"/api/quest/{model.QuestId}", apiModel);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "퀘스트 수정 실패");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // [6] 퀘스트 삭제
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync($"/api/quest/{id}");
            return RedirectToAction("Index");
        }
    }
}
