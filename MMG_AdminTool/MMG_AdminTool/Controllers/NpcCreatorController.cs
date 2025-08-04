using Microsoft.AspNetCore.Mvc;
using MMG_AdminTool.Models;

namespace MMG_AdminTool.Controllers
{
    public class NpcCreatorController : Controller
    {
        private readonly HttpClient _httpClient;

        public NpcCreatorController(IHttpClientFactory httpFactory)
        {
            _httpClient = httpFactory.CreateClient("API");
        }

        // [1] 목록 보기 
        public async Task<IActionResult> Index()
        {
            // NpcTemplate 기준으로 목록 가져오기
            var templates = await _httpClient.GetFromJsonAsync<List<NpcTemplateViewModel>>("/api/npctemplate");
            return View(templates);
        }
        // [2] 생성 - Get
        [HttpGet]
        public IActionResult Create()
        {
            return View(new NpcTemplateViewModel());
        }

        // [2] 생성 - Post
        [HttpPost]
        public async Task<IActionResult> Create(NpcTemplateViewModel model)
        {
            // 1. NPC 템플릿 생성
            var templateResponse = await _httpClient.PostAsJsonAsync("/api/npctemplate", model);
            if (!templateResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "NpcTemplate 생성 실패");
                return View(model);
            }

            var createdTemplate = await templateResponse.Content.ReadFromJsonAsync<NpcTemplateViewModel>();
            if (createdTemplate == null)
            {
                ModelState.AddModelError("", "NpcTemplate 반환 값이 잘못되었습니다.");
                return View(model);
            }

            // 2. 자동으로 Spawn도 생성
            var spawnDto = new
            {
                TemplateId = createdTemplate.TemplateId,
                MapId = 1,
                PosX = 0f,
                PosY = 0f,
                PosZ = 0f,
                DirY = 0f
            };

            await _httpClient.PostAsJsonAsync("/api/npcspawn", spawnDto);

            return RedirectToAction("Index");
        }

        // [3] 수정 - Get
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _httpClient.GetFromJsonAsync<NpcTemplateViewModel>($"/api/npctemplate/{id}");
            return View(template);
        }
        // [3] 수정 Post
        [HttpPost]
        public async Task<IActionResult> Edit(NpcTemplateViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/npctemplate/{model.TemplateId}", model);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "NpcTemplate 수정 실패");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // [4] 삭제
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // 스폰 삭제
            await _httpClient.DeleteAsync($"/api/npctemplate/{id}");
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> ManageItems(int id)
        {
            // 1. NPC Template 정보 가져오기
            var template = await _httpClient.GetFromJsonAsync<NpcTemplateViewModel>($"/api/npctemplate/{id}");

            // 2. 모든 아이템 목록 가져오기
            var items = await _httpClient.GetFromJsonAsync<List<ItemViewModel>>("/api/items");

            // 3. 기존 Shop 아이템(JsonShopItems) 로드
            var shopItems = new List<ShopItemEntry>();
            if (!string.IsNullOrEmpty(template.JsonShopItems))
            {
                shopItems = System.Text.Json.JsonSerializer.Deserialize<List<ShopItemEntry>>(template.JsonShopItems)
                             ?? new List<ShopItemEntry>();
            }

            // 4. ViewModel 구성
            var vm = new ShopItemEditViewModel
            {
                TemplateId = id,
                TemplateName = template.Name,
                AvailableItems = items,
                CurrentShopItems = shopItems
            };

            // 5. ManageItems.cshtml로 이동
            return View("ManageItems", vm);
        }

        [HttpPost]
        public async Task<IActionResult> ManageItems(ShopItemEditViewModel model)
        {
            string jsonShopItems = System.Text.Json.JsonSerializer.Serialize(model.CurrentShopItems);

            // NpcTemplate 가져오기
            var template = await _httpClient.GetFromJsonAsync<NpcTemplateViewModel>($"/api/npctemplate/{model.TemplateId}");
            template.JsonShopItems = jsonShopItems;

            // API 업데이트
            await _httpClient.PutAsJsonAsync($"/api/npctemplate/{model.TemplateId}", template);

            return RedirectToAction("Index");
        }
    }
}
