using QuestPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Quest
{
    // 퀘스트 진행도
    public class QuestCounter
    {
        public Quest ThisQuest; // 이 퀘스트에 대한 정보

        public List<QuestCountChecker> QuestGoals = new List<QuestCountChecker>();
        public QuestCounter(Quest _quest, string jsonState)
        {
            ThisQuest = _quest;

            var progressMap = string.IsNullOrEmpty(jsonState)
                ? new Dictionary<string, int>()
                : QuestProgressUtil.Parse(jsonState);

            var merged = new Dictionary<(QuestGoalType, int), QuestCountChecker>();

            foreach (var quest in ThisQuest.QuestGoals)
            {
                int _goalId;
                int _goalCount;
                if (quest.GoalType == QuestGoalType.HuntMonster)
                {
                    var q = quest as QuesthuntMonsterGoal;
                    _goalId = q!.Data.MonsterId;
                    _goalCount = q.Data.Count;
                }
                else
                {
                    var q = quest as QuestGetItemGoal;
                    _goalId = q!.Data.ItemId;
                    _goalCount = q.Data.Count;
                }
                var now = QuestProgressUtil.GetCount(progressMap, quest.GoalType, _goalId);
                if (now < 0) now = 0;

                var key = (quest.GoalType, _goalId);
                if(!merged.TryGetValue(key, out var chk))
                {
                    chk = new QuestCountChecker
                    {
                        QuestId = ThisQuest.QuestId,
                        GoalType = quest.GoalType,
                        GoalId = _goalId,
                        GoalCount = _goalCount,
                        NowCount = Math.Min(now, _goalCount),
                    };
                }
                else
                {
                    chk.GoalCount += _goalCount;
                    chk.NowCount = Math.Min(chk.NowCount, chk.GoalCount);
                }

                QuestGoals.AddRange(merged.Values);
            }
        }
        public void AddQuest(ref Dictionary<(QuestGoalType, int), List<QuestCountChecker>> dictionary)
        {
            foreach(var q in QuestGoals)
            {
                var key = (q.GoalType, q.GoalId);
                if (dictionary.TryGetValue(key, out var list) == false)
                    dictionary[key] = list = new List<QuestCountChecker>();
                list.Add(q);
            }
        }
        public void RemoveQuest(ref Dictionary<(QuestGoalType, int), List<QuestCountChecker>> dictionary)
        {
            foreach(var q in QuestGoals)
            {
                var key = (q.GoalType, q.GoalId);
                if (dictionary.TryGetValue(key, out var list) == false)
                    continue;
                list.Remove(q);

                if (list.Count == 0)
                    dictionary.Remove(key);
            }
        }
        public bool IsCompleted => QuestGoals.All(c => c.NowCount >= c.GoalCount);
    }
    public class QuestCountChecker
    {
        public int QuestId;
        public QuestGoalType GoalType;
        public int GoalId;
        public int GoalCount;
        public int NowCount;
    }
}
