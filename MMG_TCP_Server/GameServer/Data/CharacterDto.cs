using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    public class CharacterDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SlotNumber { get; set; }

        public string CharacterName { get; set; }
        public string Class { get; set; }
        public string AppearanceCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public bool isDeleted { get; set; }

    }
}
