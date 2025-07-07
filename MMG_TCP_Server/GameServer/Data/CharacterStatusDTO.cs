using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    public class CharacterStatusDTO
    {
        public int characterId { get; set; }
        public int level { get; set; }
        public int exp { get; set; }
        public int gold { get; set; }
        public int maxHp { get; set; }
        public int nowHp { get; set; }
        public int maxMp { get; set; }
        public int nowMp { get; set; }
        public DateTime lastUpdated { get; set; }
    }
}
