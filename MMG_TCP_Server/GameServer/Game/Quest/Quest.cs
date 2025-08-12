using QuestPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Quest
{
    public class Quest
    {
        public QuestInfo Info;
        public QuestCondition QuestCondition;
        public QuestAllow QuestAllow;
        public List<IQuestGoal> QuestGoals = new();
        public QuestEnd QuestEnd;
        public QuestReward QuestReward;

    }
}
