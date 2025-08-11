using GamePacket;
using GameServer.Game.Quest;
using Newtonsoft.Json;
using NPCPacket;
using QuestPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.DataManager
{
    public class QuestDataManager
    {
        public static Dictionary<int, Quest> QuestDataDictionary = new Dictionary<int, Quest>();
        public static async Task LoadQuestData()
        {
            await LoadAllQuestAsync();
        }
        public static async Task LoadAllQuestAsync()
        {
            try
            {
                using var http = new HttpClient();
                var res = await http.GetAsync(Program.URL + "/api/quest");
                var json = await res.Content.ReadAsStringAsync();
                var questDtos = JsonConvert.DeserializeObject<List<QuestDto>>(json);

                QuestDataDictionary.Clear();
                foreach (var dto in questDtos)
                {
                    Quest quest = new Quest()
                    {
                        Info = new QuestInfo()
                        {
                            Title = dto.Title,
                            Content = dto.Description,
                            IconCode = dto.IconCode ?? -1,
                            Type = (QuestType)dto.Type,
                            SortOrder = dto.SortOrder
                        },
                        QuestCondition = new QuestCondition()
                        {
                            MinLevel = dto.MinLevel,
                        },
                        QuestAllow = new QuestAllow()
                        {
                            Type = (QuestTriggerType)dto.StartTriggerType,
                            NpcId = dto.StartNpcId ?? -1,
                        },
                        QuestEnd = new QuestEnd()
                        {
                            Type = (QuestTriggerType)dto.EndTriggerType,
                            NpcId = dto.EndNpcId ?? -1,
                        },
                    };
                    if (!string.IsNullOrWhiteSpace(dto.PrevQuestIds))
                    {
                        IEnumerable<int> ids = null;

                        if (ids == null)
                        {
                            ids = dto.PrevQuestIds
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim().Trim('\"'))
                                .Where(s => int.TryParse(s, out _))
                                .Select(int.Parse);
                        }
                        quest.QuestCondition.PrevQuestId.AddRange(ids);
                    }
                    QuestDataDictionary[dto.QuestId] = quest;
                }

                Console.WriteLine($"[SkillDataManager] {QuestDataDictionary.Count}개 로드됨");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SkillDataManager] 예외 발생: {ex.Message}");
            }
        }
        public static async Task LoadQuestGoalAsync()
        {
            try
            {
                int cnt = 0;
                using var http = new HttpClient();
                var res = await http.GetAsync(Program.URL + "/api/QuestGoal");
                var json = await res.Content.ReadAsStringAsync();
                var QuestGoalDtos = JsonConvert.DeserializeObject<List<QuestGoalDto>>(json);

                foreach (var data in QuestGoalDtos)
                {
                    if (QuestDataDictionary.ContainsKey(data.QuestId))
                    {
                        if(data.GoalType == 0)
                        {

                        }

                        //NpcQuestLinkData QuestLink = new NpcQuestLinkData()
                        //{
                        //    NpcTemplateId = data.NpcTemplateId,
                        //    QuestId = data.QuestId,
                        //    LinkType = data.LinkType,
                        //};

                        //if (data.LinkType == 0)
                        //{
                        //    _npcDataDic[data.NpcTemplateId].StartQuestDataInfo.Add(QuestLink);
                        //}
                        //else if (data.LinkType == 1)
                        //{
                        //    _npcDataDic[data.NpcTemplateId].EndQuestDataInfo.Add(QuestLink);
                        //}
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
    }
    public class QuestGoalDto
    {
        public int QuestId { get; set; }
        public int GoalType { get; set; }
        public int TargetId { get; set; }
        public int Count { get; set; }
    }
    public class QuestDto
    {
        public int QuestId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? IconCode { get; set; }
        public int Type { get; set; }
        public int SortOrder { get; set; }
        public int MinLevel { get; set; }
        public string? PrevQuestIds { get; set; }
        public int StartTriggerType { get; set; }
        public int? StartNpcId { get; set; }
        public int EndTriggerType { get; set; }
        public int? EndNpcId { get; set; }

    }
}
