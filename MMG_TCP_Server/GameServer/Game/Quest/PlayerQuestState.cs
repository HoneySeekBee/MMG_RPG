using QuestPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Quest
{
    public class PlayerQuestState
    {
        public int QuestId { get; set; }
        public QuestProgressStatus Status{ get; set; }

        public Dictionary<int, int> GoalProgress { get; private set; } = new();

        public PlayerQuestState(int questId)
        {
            QuestId = questId;
            Status = QuestProgressStatus.None;
        }
        public void AddProgress(int goalIndex, int amount)
        {
            if(!GoalProgress.ContainsKey(goalIndex))
                GoalProgress[goalIndex] = 0;

            GoalProgress[goalIndex] += amount;
        }
        public int GetProgress(int goalIndex)
        {
            return GoalProgress.TryGetValue(goalIndex, out int value) ? value : 0;
        }
    }
}
