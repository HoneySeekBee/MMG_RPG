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
        public ItemInfo Info { get; set; } = new ItemInfo();
        public ItemRequirement Requirement { get; set; } = new ItemRequirement();
        public StatModifier EquipStatBonus { get; set; } = new StatModifier();
        public UseableEffect UseableEffect; 

        //public bool CanEquip(Player, Item)

        //public void UseItem(Player, Item) 
    }
}
