using GamePacket;
using GameServer.Game.Item;
using ItemPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameServer.Data.DataManager
{
    public class ItemDataManager
    {
        public static Dictionary<int, ItemData> ItemDataDictionary = new Dictionary<int, ItemData>();

        public static async Task LoadItemDataManager()
        {
            string apiUrl = $"{Program.URL}/api/items";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ItemDataManager] [LoadItemDataManager] 불러오기 실패 {response.StatusCode}");
                return;
            }
            string json = await response.Content.ReadAsStringAsync();
            var entities = JsonSerializer.Deserialize<List<ItemEntity>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (entities == null)
            {
                Console.WriteLine("[ItemDataManager] 역직렬화 실패");
                return;
            }
            foreach (var entity in entities)
            {
                var itemData = new ItemData();

                // 1. 기본 정보
                itemData.Info = new ItemInfo
                {
                    ItemId = entity.ItemId,
                    Name = entity.Name,
                    Description = entity.Description,
                    Type = (ItemType)entity.Type,
                    ItemIconId = entity.IconId ?? 0,
                    ItemModelId = entity.ModelId ?? 0
                };

                // 2, 장착 조건 
                itemData.Requirement = new ItemRequirement
                {
                    RequiredLevel = entity.RequiredLevel ?? 0
                };
                if (!string.IsNullOrEmpty(entity.JsonRequiredStats))
                {
                    try
                    {
                        var dto = JsonSerializer.Deserialize<RequiredStatsJsonDto>(entity.JsonRequiredStats);
                        if (dto != null)
                        {
                            if (dto.required_status != null)
                                itemData.Requirement.RequiredStatus.AddRange(dto.required_status);

                            if (dto.classTypes != null)
                            {
                                foreach (int jobInt in dto.classTypes)
                                {
                                    if (Enum.IsDefined(typeof(ClassType), jobInt))
                                        itemData.Requirement.AllowJobs.Add((ClassType)jobInt);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ItemDataManager] RequiredStats 역직렬화 실패: {ex.Message}");
                    }
                }

                // 3. 장비 능력치
                if (!string.IsNullOrEmpty(entity.JsonStatModifiers))
                {
                    try
                    {
                        var statModifiers = JsonSerializer.Deserialize<List<StatData>>(entity.JsonStatModifiers);
                        if (statModifiers != null)
                            itemData.EquipStatBonus.StatModifiers.AddRange(statModifiers);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ItemDataManager] StatModifiers 역직렬화 실패: {ex.Message}");
                    }
                }

                // 4. 사용 효과
                if (!string.IsNullOrEmpty(entity.JsonUseableEffect))
                {
                    try
                    {
                        var useableEffect = JsonSerializer.Deserialize<UseableEffect>(entity.JsonUseableEffect);
                        if (useableEffect != null)
                            itemData.UseableEffect = useableEffect;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ItemDataManager] UseableEffect 역직렬화 실패: {ex.Message}");
                    }
                }

                // 5. 딕셔너리에 추가
                ItemDataDictionary[itemData.Info.ItemId] = itemData;
            }
            Console.WriteLine($"[ItemDataManager] {ItemDataDictionary.Count}개 로드됨");
        }

        public static ItemData? Get(int id)
        {
            return ItemDataDictionary.TryGetValue(id, out var data) ? data : null;
        }
    }
    public class ItemEntity
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }

        public int? IconId { get; set; }
        public int? ModelId { get; set; }

        public int? RequiredLevel { get; set; }
        public string? JsonStatModifiers { get; set; }
        public string? JsonRequiredStats { get; set; }
        public string? JsonUseableEffect { get; set; }
    }
    public class RequiredStatsJsonDto
    {
        public List<StatData> required_status { get; set; } = new List<StatData>();
        public List<int> classTypes { get; set; } = new List<int>();
    }
}
