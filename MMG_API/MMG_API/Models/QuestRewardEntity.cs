using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MMG_API.Models
{
    [Table("QuestReward")]
    public class QuestRewardEntity
    {
        [Key]
        public int QuestId { get; set; }
        public int Exp { get; set; }
        public string JsonReward { get; set; }
    }
}
