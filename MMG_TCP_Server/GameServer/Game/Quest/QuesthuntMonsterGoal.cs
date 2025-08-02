using QuestPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Quest
{
    public class QuesthuntMonsterGoal : IQuestGoal
    {
        public QuestGoalType GoalType => QuestGoalType.HuntMonster;
        public QuestHuntMonsterGoalData Data = new();

        public bool IsCompleted(PlayerQuestState State)
        {
            int current = State.GetProgress(Data.MonsterId);
            return current >= Data.Count;
        }
    }
}
