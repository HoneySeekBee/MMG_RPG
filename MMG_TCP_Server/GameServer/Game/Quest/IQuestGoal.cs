using QuestPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Quest
{
    public interface IQuestGoal
    {
        QuestGoalType GoalType { get; }
        bool IsCompleted(PlayerQuestState State);
    }
}
