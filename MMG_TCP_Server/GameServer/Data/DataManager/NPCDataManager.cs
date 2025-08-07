using Google.Protobuf.WellKnownTypes;
using MonsterPacket;
using Newtonsoft.Json;
using NPCPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.DataManager
{
    public class NPCDataManager
    {
        private static Dictionary<int, NPCData> _npcDataDic = new();

        public static async Task LoadNpcData()
        {
            await LoadAllNpcTemplateAsync();
            await LoadAllNpcQuestLinkAsync();
            await LoadAllNpcSpawnAsync();
        }

        public static async Task LoadAllNpcTemplateAsync()
        {
            // 가지고 있는 모든 NPCTemplate 가지고 오기 
            try
            {
                using var http = new HttpClient();
                // [1] Npc Template 정보 가지고 오기 
                var res = await http.GetAsync(Program.URL + "/api/NpcTemplate");
                var json = await res.Content.ReadAsStringAsync();
                var NpcTemplateDtos = JsonConvert.DeserializeObject<List<NpcTemplateDto>>(json);

                foreach (var data in NpcTemplateDtos)
                {
                    _npcDataDic[data.TemplateId] = new NPCData()
                    {
                        info = new NpcInfo()
                        {
                            NpcId = data.TemplateId,
                            Name = data.Name,
                            Type = (NPCType)data.Type,
                            DialogKey = data.DialogueKey?? string.Empty,
                            ShopItem = data.JsonShopItems?? string.Empty,
                        },
                        StartQuestDataInfo = new List<NpcQuestLinkData>(),
                        EndQuestDataInfo = new List<NpcQuestLinkData>(),
                        SpawnData = new(),
                    };
                }
                Console.WriteLine($"[NPCDatamanager] {_npcDataDic.Count}개 로드 완료 ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NPCDatamanager] 예외 발생: {ex.Message}");
            }
        }
        public static async Task LoadAllNpcQuestLinkAsync()
        {
            try
            {
                int cnt = 0;
                using var http = new HttpClient();
                var res = await http.GetAsync(Program.URL + "/api/NpcQuestLink");
                var json = await res.Content.ReadAsStringAsync();
                var NpcQuestLinkDtos = JsonConvert.DeserializeObject<List<NPCQuestLinkDto>>(json);

                foreach (var data in NpcQuestLinkDtos)
                {
                    if (_npcDataDic.ContainsKey(data.NpcTemplateId))
                    {
                        NpcQuestLinkData QuestLink = new NpcQuestLinkData()
                        {
                            NpcTemplateId = data.NpcTemplateId,
                            QuestId = data.QuestId,
                            LinkType = data.LinkType,
                        };

                        if (data.LinkType == 0)
                        {
                            _npcDataDic[data.NpcTemplateId].StartQuestDataInfo.Add(QuestLink);
                        }
                        else if (data.LinkType == 1)
                        {
                            _npcDataDic[data.NpcTemplateId].EndQuestDataInfo.Add(QuestLink);
                        }
                        cnt++;
                    }
                    else
                    {
                        Console.WriteLine($"[Error] [LoadAllNpcQuestLink] 중 NPC {data.NpcTemplateId}가 존재하지 않음 ");
                    }
                }
                Console.WriteLine($"[NPCQuestLink] {cnt}개 업데이트 완료 ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NPCQuestLink] 예외 발생: {ex.Message}");
            }
        }
        public static async Task LoadAllNpcSpawnAsync()
        {
            try
            {
                int cnt = 0;
                using var http = new HttpClient();
                var res = await http.GetAsync(Program.URL + "/api/NpcSpawn");
                var json = await res.Content.ReadAsStringAsync();
                var NpcSpawnDtos = JsonConvert.DeserializeObject<List<NPCSpawnDto>>(json);

                foreach (var data in NpcSpawnDtos)
                {
                    if (_npcDataDic.ContainsKey(data.TemplateId))
                    {
                        NpcSpawnData npcSpawnData = new NpcSpawnData()
                        {
                            SpawnId = data.SpawnId,
                            TemplateId = data.TemplateId,
                            MapId = data.MapId,
                            PosX = data.PosX,
                            PosY = data.PosY,
                            PosZ = data.PosZ,
                            DirY = data.DirY,
                        };
                        _npcDataDic[data.TemplateId].SpawnData = npcSpawnData;
                        cnt++;
                    }
                }
                Console.WriteLine($"[NpcSpawnData] {cnt}개 업데이트 완료 ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Get - NPCSpawnPosition] 예외 발생: {ex.Message}");
            }
        }
    }
    public class NPCSpawnDto
    {
        public int SpawnId { get; set; }
        public int TemplateId { get; set; }
        public int MapId { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int PosZ { get; set; }
        public int DirY { get; set; }
    }
    public class NPCQuestLinkDto
    {
        public int NpcTemplateId { get; set; }
        public int QuestId { get; set; }
        public int LinkType { get; set; }
    }
    public class NpcTemplateDto
    {
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string DialogueKey { get; set; }
        public string JsonShopItems { get; set; }

    }
    public class NPCData
    {
        public NpcInfo info;
        public List<NpcQuestLinkData> StartQuestDataInfo = new List<NpcQuestLinkData>();
        public List<NpcQuestLinkData> EndQuestDataInfo = new List<NpcQuestLinkData>();
        public NpcSpawnData SpawnData;
        // 이후에 Quest를 시작하거나 종료하는 정보
        // 어느 마을에서 어느 위치에 생성되는지
    }
}
