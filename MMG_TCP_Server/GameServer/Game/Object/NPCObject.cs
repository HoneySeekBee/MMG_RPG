using GamePacket;
using NPCPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Object
{
    public class NPCObject : GameObject
    {
        public NpcInfo NPCInfo;

        // 퀘스트 관련
        public List<int> StartableQuestId = new();
        public List<int> CompletableQuestId = new();

        // 상점 
        public List<int> ShopItemIds = new();

        public override void OnDeath()
        {
            Console.WriteLine("[NPCObject] NPC가 사망하였습니다. ");
        }
        //public List<int> GetAvailableQuest(Player, Npc);
    }
}
