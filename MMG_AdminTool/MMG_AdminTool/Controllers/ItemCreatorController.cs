using Microsoft.AspNetCore.Mvc;
using MMG_AdminTool.Models;

namespace MMG_AdminTool.Controllers
{
    public class ItemCreatorController : Controller
    {
        private readonly HttpClient _httpClient;

        public ItemCreatorController(IHttpClientFactory httpFactory)
        {
            _httpClient = httpFactory.CreateClient("API");
        }
        // [1] 아이템 목록 보기 (Read)
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetFromJsonAsync<List<ItemViewModel>>("/api/items");
            return View(response);
        }
        // [2] 아이템 수정하기 (Update)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _httpClient.GetFromJsonAsync<ItemViewModel>($"/api/items/{id}");
            return View(item);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ItemViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/items/{model.ItemId}", model);
            return RedirectToAction("Index");
        }
        private void SetSpriteFiles()
        {
            string spriteFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Sprite");
            List<string> files = new List<string>();
            if (Directory.Exists(spriteFolder))
            {
                files = Directory.GetFiles(spriteFolder)
                                 .Select(Path.GetFileName)
                                 .ToList();
            }
            ViewBag.SpriteFiles = files;
        }
        // [3] 아이템을 생성 (Create)
        [HttpGet]
        public IActionResult Create()
        {
            SetSpriteFiles();
            return View(new ItemViewModel());
        }
        string? NormalizeJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var trimmed = json.Trim();
            if (trimmed == "{}")
                return null;

            // ClassTypes만 있는 JSON인지 검사
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(trimmed);
                var root = doc.RootElement;

                // 속성이 ClassTypes 하나뿐이고 배열이 비어 있다면 null 처리
                if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    var props = root.EnumerateObject().ToList();
                    if (props.Count == 1 &&
                        props[0].Name == "ClassTypes" &&
                        props[0].Value.ValueKind == System.Text.Json.JsonValueKind.Array &&
                        !props[0].Value.EnumerateArray().Any())
                    {
                        return null;
                    }
                }
            }
            catch
            {
                // JSON 파싱 실패 시 그냥 원본 반환
            }

            return trimmed;
        }

        [HttpPost]
        public async Task<IActionResult> Create(ItemViewModel model)
        {
            // DB에 보낼 DTO (SelectedIcon은 제외)
            var dto = new
            {
                Name = model.Name,
                Description = model.Description,
                Type = model.Type,
                RequiredLevel = model.RequiredLevel,
                JsonStatModifiers = NormalizeJson(model.JsonStatModifiers),
                JsonRequiredStats = NormalizeJson(model.JsonRequiredStats),
                JsonUseableEffect = NormalizeJson(model.JsonUseableEffect)
            };

            // 1. API 호출
            var response = await _httpClient.PostAsJsonAsync("/api/items", dto);

            // 1-1. API 응답 확인
            var raw = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                SetSpriteFiles(); // 다시 세팅
                ModelState.AddModelError(string.Empty, $"아이템 생성 실패: {response.StatusCode}");
                return View(model);
            }
            var createdItem = System.Text.Json.JsonSerializer.Deserialize<ItemViewModel>(raw);
            if (createdItem == null)
            {
                SetSpriteFiles();
                ModelState.AddModelError(string.Empty, "아이템 생성에 실패했습니다.");
                return View(model);
            }


            // 3. IconId/ModelId 설정
            createdItem.IconId = createdItem.ItemId;
            createdItem.ModelId = (createdItem.Type == 0) ? createdItem.ItemId : -1;

            // 4. API에 업데이트 (PUT)
            await _httpClient.PutAsJsonAsync($"/api/items/{createdItem.ItemId}", createdItem);

            // 5. 아이콘 이미지 복사
            if (!string.IsNullOrEmpty(model.SelectedIcon))
            {
                string spriteFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Sprite");
                string sourceFile = Path.Combine(spriteFolder, model.SelectedIcon);

                string targetFolder = @"C:\Users\USER\OneDrive\바탕 화면\MMG\MMG_RPG\MMG_RPG\MMG_Client\Assets\Art\Sprite\Icon";
                string targetFile = Path.Combine(targetFolder, $"Item_Icon_{createdItem.IconId}{Path.GetExtension(sourceFile)}");

                System.IO.File.Copy(sourceFile, targetFile, overwrite: true);
            }

            return RedirectToAction("Index");
        }

        // [4] 삭제
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync($"/api/items/{id}");
            return RedirectToAction("Index");
        }
    }
}
