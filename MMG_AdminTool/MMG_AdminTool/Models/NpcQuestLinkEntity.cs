using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MMG_AdminTool.Models
{
    [Table("NpcQuestLinkTable")]
    public class NpcQuestLinkEntity
    {
        [Key, Column(Order = 0)]
        public int NpcTemplateId { get; set; }

        [Key, Column(Order = 1)]
        public int QuestId { get; set; }

        // 0 = Start, 1 = End
        public int LinkType { get; set; }
    }
}
