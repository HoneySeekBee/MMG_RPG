using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MMG_API.Models
{
    [Table("CharacterStatus")]
    public class CharacterStatus
    {
        [Key]
        public int CharacterId { get; set; }

        public int CharacterLevel { get; set; }

        public float Exp { get; set; }

        public float MaxExp { get; set; }
        public int Gold { get; set; }

        public float HP { get; set; }


        public float MP { get; set; }


        public float NowHP { get; set; }


        public float NowMP { get; set; }
        public DateTime LastUpdateAt { get; set; } = DateTime.UtcNow;

    }
    public class UpdateCharacterStatusRequest
    {
        public int CharacterId { get; set; }
        public int? CharacterLevel { get; set; }
        public float? HP { get; set; }
        public float? MP { get; set; }
        public float? NowHP { get; set; }
        public float? NowMP { get; set; }
        public float? Exp { get; set; }
        public float? MaxExp{ get; set; }
        public int? Gold { get; set; }
    }
}