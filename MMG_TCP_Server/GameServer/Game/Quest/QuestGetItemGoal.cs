using QuestPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Quest
{
    public class QuestGetItemGoal : IQuestGoal
    {
        public QuestGoalType GoalType => QuestGoalType.GetItem;
        public QuestGetItemGoalData Data = new();

        public bool IsCompleted(PlayerQuestState State)
        {
            int current = State.GetProgress(Data.ItemId);
            return current >= Data.Count;
        }
    }
}
