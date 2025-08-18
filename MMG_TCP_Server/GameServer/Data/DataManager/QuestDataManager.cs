using GamePacket;
using GameServer.Game.Quest;
using NPCPacket;
using QuestPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameServer.Data.DataManager
{
    public class QuestDataManager
    {
        public static Dictionary<int, Quest> QuestDataDictionary = new Dictionary<int, Quest>();
        public static async Task LoadQuestData()
        {
            await LoadAllQuestAsync();
            await LoadQuestGoalAsync();
            await LoadQuestRewardAsync();
        }
        public static async Task LoadAllQuestAsync()
        {
            try
            {
                using var http = new HttpClient();
                var res = await http.GetAsync(Program.URL + "/api/quest");
                var json = await res.Content.ReadAsStringAsync();
                var questDtos = JsonSerializer.Deserialize<List<QuestDto>>(json,
           new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
           ?? new List<QuestDto>();

                QuestDataDictionary.Clear();
                foreach (var dto in questDtos)
                {
                    Quest quest = new Quest()
                    {
                        QuestId = dto.QuestId,
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

                Console.WriteLine($"[LoadAllQuestAsync] {QuestDataDictionary.Count}개 로드됨");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadAllQuestAsync] 예외 발생: {ex.Message}");
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
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var questGoalDtos = JsonSerializer.Deserialize<List<QuestGoalDto>>(json, opts) ?? new List<QuestGoalDto>();

                foreach (var data in questGoalDtos)
                {
                    if (QuestDataDictionary.ContainsKey(data.QuestId))
                    {
                        if (data.GoalType == 0)
                        {
                            QuesthuntMonsterGoal HuntMonster = new QuesthuntMonsterGoal()
                            {
                                Data = new QuestHuntMonsterGoalData()
                                {
                                    MonsterId = data.TargetId,
                                    Count = data.Count
                                }
                            };
                            QuestDataDictionary[data.QuestId].QuestGoals.Add(HuntMonster);
                        }
                        else
                        {
                            QuestGetItemGoal GetItem = new QuestGetItemGoal()
                            {
                                Data = new QuestGetItemGoalData()
                                {
                                    ItemId = data.TargetId,
                                    Count = data.Count
                                }
                            };
                            QuestDataDictionary[data.QuestId].QuestGoals.Add(GetItem);
                        }
                        cnt++;
                    }
                    else
                    {
                        Console.WriteLine($"[Error] [LoadQuestGoalAsync] 중 quest {data.QuestId}가 존재하지 않음 ");
                    }
                }
                Console.WriteLine($"[LoadQuestGoalAsync] {cnt}개 업데이트 완료 ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadQuestGoalAsync] 예외 발생: {ex.Message}");
            }
        }
        public static async Task LoadQuestRewardAsync()
        {
            try
            {
                int cnt = 0;
                using var http = new HttpClient();
                var res = await http.GetAsync(Program.URL + "/api/QuestReward");
                var json = await res.Content.ReadAsStringAsync();

                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var QuestRewardsDtos = JsonSerializer.Deserialize<List<QuestRewardDto>>(json, opts) ?? new List<QuestRewardDto>();

                foreach (var data in QuestRewardsDtos)
                {
                    if (QuestDataDictionary.ContainsKey(data.QuestId))
                    {
                        List<RewardItem> rewardList = new();
                        if (!string.IsNullOrWhiteSpace(data.JsonReward))
                        {
                            try
                            {
                                rewardList = JsonSerializer.Deserialize<List<RewardItem>>(
                                    data.JsonReward,
                                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                                ) ?? new();
                            }
                            catch (JsonException ex)
                            {
                                Console.WriteLine($"[QuestReward] JSON parse error: {ex.Message}");
                            }
                        }
                        var protoRewardItems = rewardList.Select(r => new RewardItem
                        {
                            ItemId = r.ItemId,
                            Count = r.Count
                        }).ToList();
                        QuestDataDictionary[data.QuestId].QuestReward = new QuestReward()
                        {
                            RewardItems = { protoRewardItems },
                            Exp = data.Exp,
                        };
                        cnt++;
                    }
                    else
                    {
                        Console.WriteLine($"[LoadQuestRewardAsync] {data.QuestId}가 포함되지 않음");
                    }
                }
                Console.WriteLine($"[LoadQuestRewardAsync] {cnt}개 업데이트 완료 ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadQuestRewardAsync] 예외 발생: {ex.Message}");
            }
        }

    
        public static List<Quest> GetQuest_byCondition(int myLevel, ISet<int> completedQuests)
        {
            var completedSet = new HashSet<int>(completedQuests);

            return QuestDataDictionary.Values
                .Where(q =>
                q.QuestCondition.MinLevel <= myLevel &&
                (q.QuestCondition.PrevQuestId == null ||
                q.QuestCondition.PrevQuestId.All(pid => completedSet.Contains(pid))))
                .ToList();
        }
    }
    public class QuestRewardDto
    {
        public int QuestId { get; set; }
        public int Exp { get; set; }
        public string JsonReward { get; set; }
    }
    public class QuestGoalDto
    {
        public int GoalIndex { get; set; }
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
