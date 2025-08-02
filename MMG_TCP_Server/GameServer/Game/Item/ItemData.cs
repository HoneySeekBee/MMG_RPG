using GamePacket;
using ItemPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Item
{
    public class ItemData
    {
        public ItemInfo Info;
        public ItemRequirement Requirement;
        public StatModifier EquipStatBonus;
        public UseableEffect UseableEffect; 

        //public bool CanEquip(Player, Item)

        //public void UseItem(Player, Item) 
    }
}
